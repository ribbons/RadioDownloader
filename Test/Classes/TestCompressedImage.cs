/*
 * This file is part of Radio Downloader.
 * Copyright © 2019-2020 by the authors - see the AUTHORS file for details.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace RadioDldTest
{
    using System;
    using System.Drawing;
    using System.IO;

    using RadioDld;
    using Xunit;

    /// <summary>
    /// Tests for the CompressedImage class.
    /// </summary>
    public class TestCompressedImage
    {
        /// <summary>
        /// Test that GetBytes returns the exact compressed data passed to the ctor.
        /// </summary>
        [Fact]
        public void UnchangedBytes()
        {
            var data = File.ReadAllBytes("TestData/valid_image.jpg");
            var image = new CompressedImage(data);

            Assert.Equal(data, image.GetBytes());
        }

        /// <summary>
        /// Test that the ctor throws an ArgumentException when passed non-image data.
        /// </summary>
        [Fact]
        public void InvalidImage()
        {
            Assert.Throws<ArgumentException>(() =>
                new CompressedImage(File.ReadAllBytes("TestData/invalid_image")));
        }

        /// <summary>
        /// Test that the image width and height are set correctly.
        /// </summary>
        [Fact]
        public void Dimensions()
        {
            var image = new CompressedImage(File.ReadAllBytes("TestData/valid_image.jpg"));

            Assert.Equal(58, image.Width);
            Assert.Equal(61, image.Height);
        }

        /// <summary>
        /// Test that the image content is returned correctly.
        /// </summary>
        [Fact]
        public void ImageContent()
        {
            const string path = "TestData/valid_image.jpg";
            var compressed = new CompressedImage(File.ReadAllBytes(path));

            using (var fileBitmap = new Bitmap(path))
            {
                using (Bitmap fromCompressed = (Bitmap)compressed.Image)
                {
                    for (int column = 0; column < fileBitmap.Width; column++)
                    {
                        for (int row = 0; row < fileBitmap.Height; row++)
                        {
                            Assert.Equal(
                                fileBitmap.GetPixel(column, row),
                                fromCompressed.GetPixel(column, row));
                        }
                    }
                }
            }
        }
    }
}
