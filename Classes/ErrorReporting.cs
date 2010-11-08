// Utility to automatically download radio programmes, using a plugin framework for provider specific implementation.
// Copyright © 2007-2010 Matt Robinson
//
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General
// Public License as published by the Free Software Foundation; either version 2 of the License, or (at your
// option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
// implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
// License for more details.
//
// You should have received a copy of the GNU General Public License along with this program; if not, write
// to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

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
    using System.Web;
    using System.Xml.Serialization;
    using Microsoft.VisualBasic;
    using Microsoft.VisualBasic.ApplicationServices;

    internal class ErrorReporting
    {
        private Dictionary<string, string> fields = new Dictionary<string, string>();

        public ErrorReporting(string errorText, string errorDetails)
        {
            try
            {
                this.fields.Add("version", new ApplicationBase().Info.Version.ToString());
                this.fields.Add("errortext", errorText);
                this.fields.Add("errordetails", errorDetails);

                string loadedAssemblies = string.Empty;

                foreach (Assembly loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    loadedAssemblies += loadedAssembly.GetName().Name + Constants.vbCrLf;
                    loadedAssemblies += "Assembly Version: " + loadedAssembly.GetName().Version.ToString() + Constants.vbCrLf;
                    loadedAssemblies += "CodeBase: " + loadedAssembly.CodeBase + Constants.vbCrLf + Constants.vbCrLf;
                }

                this.fields.Add("loadedassemblies", loadedAssemblies);
            }
            catch
            {
                // No way of reporting errors that have happened here, so just give up
            }
        }

        public ErrorReporting(string errorText, string errorDetails, Dictionary<string, string> extraFields)
            : this(errorText, errorDetails)
        {
            try
            {
                foreach (KeyValuePair<string, string> extraItem in extraFields)
                {
                    this.fields.Add(extraItem.Key, extraItem.Value);
                }
            }
            catch
            {
                // No way of reporting errors that have happened here, so just give up
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
                // No way of reporting errors that have happened here, so just give up
            }
        }

        public ErrorReporting(Exception uncaughtException)
            : this(uncaughtException.GetType().ToString() + ": " + uncaughtException.Message, uncaughtException.GetType().ToString() + Constants.vbCrLf + uncaughtException.StackTrace)
        {
            try
            {
                if (object.ReferenceEquals(uncaughtException.GetType(), typeof(System.Data.SQLite.SQLiteException)))
                {
                    // Add extra information to the exception to help debug sqlite concurrency
                    uncaughtException = SQLiteMonDataReader.AddReadersInfo(uncaughtException);
                    uncaughtException = SQLiteMonTransaction.AddTransactionsInfo(uncaughtException);
                }

                this.fields.Add("exceptiontostring", uncaughtException.ToString());

                // Set up a list of types which do not need to be serialized
                List<Type> notSerialize = new List<Type>();
                notSerialize.AddRange(new Type[]
                {
                    typeof(string),
                    typeof(int),
                    typeof(float),
                    typeof(double),
                    typeof(bool)
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
                        if (overloadChkProp.Equals(thisExpProperty) == false)
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
                        string fieldName = "expdata:" + thisExpProperty.Name;
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
                            if (notSerialize.Contains(propertyValue.GetType()) == false)
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
                        if (object.ReferenceEquals(dataEntry.Key.GetType(), typeof(string)) & object.ReferenceEquals(dataEntry.Value.GetType(), typeof(string)))
                        {
                            this.fields.Add("expdata:Data:" + (string)dataEntry.Key, (string)dataEntry.Value);
                        }
                    }
                }
            }
            catch
            {
                // No way of reporting errors that have happened here, so just give up
            }
        }

        public override string ToString()
        {
            string stringValue = string.Empty;

            try
            {
                foreach (KeyValuePair<string, string> reportField in this.fields)
                {
                    stringValue += reportField.Key + ": " + reportField.Value + Constants.vbCrLf;
                }
            }
            catch
            {
                // No way of reporting errors that have happened here, so just give up
            }

            return stringValue;
        }

        public void SendReport(string sendUrl)
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

                byte[] result = sendClient.UploadData(sendUrl, "POST", System.Text.Encoding.ASCII.GetBytes(postData));
                string[] returnLines = Strings.Split(System.Text.Encoding.ASCII.GetString(result), Constants.vbLf);

                if (returnLines[0] == "success")
                {
                    Interaction.MsgBox("Your error report was sent successfully.", MsgBoxStyle.Information);

                    if (returnLines[1].StartsWith("http://", StringComparison.Ordinal) | returnLines[1].StartsWith("https://", StringComparison.Ordinal))
                    {
                        Process.Start(returnLines[1]);
                    }
                }
            }
            catch
            {
                // No way of reporting errors that have happened here, so just give up
            }
        }
    }
}
