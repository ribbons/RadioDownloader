/*
 * Copyright © 2007-2020 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

namespace RadioDld
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    [Serializable]
    internal class ErrorReporting
    {
        private const string SendReportUrl = "https://nerdoftheherd.com/tools/radiodld/error_report.php";
        private Dictionary<string, string> fields = new Dictionary<string, string>();

        public ErrorReporting(string errorText, string errorDetails)
        {
            try
            {
                this.fields.Add("version", Application.ProductVersion);
                this.fields.Add("errortext", errorText);
                this.fields.Add("errordetails", errorDetails);
                this.fields.Add("operatingsystem", Environment.OSVersion.VersionString);
                this.fields.Add("architecture", IntPtr.Size == 8 ? "x64" : "x86");
                this.fields.Add("applicationuptime", (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds.ToString("0", CultureInfo.InvariantCulture));

                List<string> loadedAssemblies = new List<string>();

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    AssemblyName name = assembly.GetName();
                    loadedAssemblies.Add(name.Name + " " + name.Version.ToString() + (assembly.GlobalAssemblyCache ? " (GAC)" : string.Empty));
                }

                loadedAssemblies.Sort();
                this.fields.Add("Loaded Assemblies", string.Join("\r\n", loadedAssemblies.ToArray()));

                // Fetch this value last as it accesses the database and is most likely to fail
                this.fields.Add("userid", Settings.UniqueUserId);
            }
            catch
            {
                // No way of reporting errors that have happened here, so try to continue
            }
        }

        public ErrorReporting(string errorPrefix, Exception uncaughtException)
            : this(uncaughtException)
        {
            try
            {
                this.fields["errortext"] = errorPrefix + ": " + this.fields["errortext"];
            }
            catch
            {
                // No way of reporting errors that have happened here, so try to continue
            }
        }

        public ErrorReporting(Exception uncaughtException)
            : this(InvariantMessage(uncaughtException), InvariantStackTrace(uncaughtException))
        {
            try
            {
                if (ReferenceEquals(uncaughtException.GetType(), typeof(System.Data.SQLite.SQLiteException)))
                {
                    // Add extra information to the exception to help debug sqlite concurrency
                    uncaughtException = SQLiteMonDataReader.AddReadersInfo(uncaughtException);
                    uncaughtException = SQLiteMonTransaction.AddTransactionsInfo(uncaughtException);
                }

                this.fields.Add("Exception.ToString()", uncaughtException.ToString());

                // Set up a list of types which do not need to be serialized
                List<Type> notSerialize = new List<Type>(new Type[]
                {
                    typeof(string),
                    typeof(int),
                    typeof(float),
                    typeof(double),
                    typeof(bool),
                });

                // Store the type of the exception and get a list of its properties to loop through
                Type exceptionType = uncaughtException.GetType();
                PropertyInfo[] baseExceptionProperties = typeof(Exception).GetProperties();

                bool extraProperty = false;
                bool overloadedProp = false;

                foreach (PropertyInfo thisExpProperty in exceptionType.GetProperties())
                {
                    extraProperty = true;
                    overloadedProp = false;

                    // Check if this property exists in the base exception class: if not then add it to the report
                    foreach (PropertyInfo baseProperty in baseExceptionProperties)
                    {
                        if (thisExpProperty.Name == baseProperty.Name)
                        {
                            extraProperty = false;
                            break;
                        }
                    }

                    // Test to see if this property is overloaded
                    foreach (PropertyInfo overloadChkProp in exceptionType.GetProperties())
                    {
                        if (!overloadChkProp.Equals(thisExpProperty))
                        {
                            if (overloadChkProp.Name == thisExpProperty.Name)
                            {
                                overloadedProp = true;
                                break;
                            }
                        }
                    }

                    if (extraProperty)
                    {
                        string fieldName = "Exception." + thisExpProperty.Name;
                        object propertyValue = thisExpProperty.GetValue(uncaughtException, null);

                        if (overloadedProp)
                        {
                            string typeName = propertyValue.GetType().ToString();
                            int dotPos = typeName.LastIndexOf(".", StringComparison.Ordinal);

                            if (dotPos >= 0)
                            {
                                typeName = typeName.Substring(dotPos + 1);
                            }

                            fieldName += ":" + typeName;
                        }

                        if (propertyValue != null && !string.IsNullOrEmpty(propertyValue.ToString()))
                        {
                            if (propertyValue.GetType() == typeof(Provider.ErrorType))
                            {
                                // ErrorType is always set to UnknownError on DownloadExceptions
                                continue;
                            }

                            if (!notSerialize.Contains(propertyValue.GetType()))
                            {
                                if (propertyValue.GetType().IsSerializable)
                                {
                                    // Attempt to serialize the object as an XML string
                                    try
                                    {
                                        StringWriter valueStringWriter = new StringWriter(CultureInfo.InvariantCulture);
                                        XmlSerializer valueSerializer = new XmlSerializer(propertyValue.GetType());

                                        valueSerializer.Serialize(valueStringWriter, propertyValue);
                                        this.fields.Add(fieldName, valueStringWriter.ToString());

                                        continue;
                                    }
                                    catch (NotSupportedException)
                                    {
                                        // Not possible to serialize - do nothing & fall through to the ToString code
                                    }
                                    catch (InvalidOperationException)
                                    {
                                        // Problem serializing the object - do nothing & fall through to the ToString code
                                    }
                                }
                            }

                            this.fields.Add(fieldName, propertyValue.ToString());
                        }
                    }
                }

                if (uncaughtException.Data != null)
                {
                    foreach (DictionaryEntry dataEntry in uncaughtException.Data)
                    {
                        if (ReferenceEquals(dataEntry.Key.GetType(), typeof(string)) && ReferenceEquals(dataEntry.Value.GetType(), typeof(string)))
                        {
                            this.fields[(string)dataEntry.Key] = (string)dataEntry.Value;
                        }
                    }
                }
            }
            catch
            {
                // No way of reporting errors that have happened here, so try to continue
            }
        }

        public override string ToString()
        {
            string stringValue = string.Empty;

            try
            {
                foreach (KeyValuePair<string, string> reportField in this.fields)
                {
                    stringValue += reportField.Key + ": " + reportField.Value + "\r\n";
                }
            }
            catch
            {
                // No way of reporting errors that have happened here, so try to continue
            }

            return stringValue;
        }

        public bool SendReport()
        {
            try
            {
                WebClient sendClient = new WebClient();
                sendClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                string postData = string.Empty;

                foreach (KeyValuePair<string, string> reportField in this.fields)
                {
                    postData += "&" + HttpUtility.UrlEncode(reportField.Key) + "=" + HttpUtility.UrlEncode(reportField.Value);
                }

                postData = postData.Substring(1);

                byte[] result = sendClient.UploadData(SendReportUrl, "POST", System.Text.Encoding.ASCII.GetBytes(postData));
                string[] returnLines = System.Text.Encoding.ASCII.GetString(result).Split('\n');

                if (returnLines[0] == "success")
                {
                    string successMessage = "Your error report was sent successfully.";

                    if (returnLines.Length > 1)
                    {
                        int reportNum;

                        if (int.TryParse(returnLines[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out reportNum))
                        {
                            successMessage += Environment.NewLine + Environment.NewLine + "The report number is " + reportNum.ToString(CultureInfo.CurrentCulture) + ".";
                        }
                    }

                    MessageBox.Show(successMessage, Application.ProductName);

                    Uri moreInfo;

                    if (Uri.TryCreate(returnLines[1], UriKind.Absolute, out moreInfo))
                    {
                        OsUtils.LaunchUrl(moreInfo, "Error Report");
                    }

                    return true;
                }
            }
            catch
            {
                // No way of reporting errors that have happened here, so just give up
            }

            return false;
        }

        /// <summary>
        /// Fetch the message for the specified exception, but in the invariant culture (if it is
        /// localised when the property is accessed rather than when the exception was created).
        /// </summary>
        /// <param name="exp">The exception to fetch the message for.</param>
        /// <returns>The exception message.</returns>
        private static string InvariantMessage(Exception exp)
        {
            // Store the current UI culture
            CultureInfo culture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                // Switch to the invariant culture and return the message
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                return exp.Message;
            }
            finally
            {
                // Switch back to the correct UI culture
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }

        /// <summary>
        /// Fetch a stack trace for the specified exception, but in the invariant culture.
        /// </summary>
        /// <param name="exp">The exception to generate a stack trace for.</param>
        /// <returns>The generated stack trace.</returns>
        private static string InvariantStackTrace(Exception exp)
        {
            // Store the current UI culture
            CultureInfo culture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                // Switch to the invariant culture and return the stack trace
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                return exp.GetType().ToString() + "\r\n"
                       + new StackTrace(exp, true).ToString();
            }
            finally
            {
                // Switch back to the correct UI culture
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }
    }
}
