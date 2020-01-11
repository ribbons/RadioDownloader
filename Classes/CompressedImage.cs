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

namespace RadioDld
{
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// Class for storing images in their compressed form (e.g. jpeg, png).
    /// </summary>
    public class CompressedImage
    {
        private byte[] imageData;
        private Image image = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedImage"/> class.
        /// </summary>
        /// <param name="imageData">The compressed image data.</param>
        public CompressedImage(byte[] imageData)
            : this(imageData, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedImage"/> class,
        /// optionally without loading the image to validate it.
        /// </summary>
        /// <param name="imageData">The compressed image data.</param>
        /// <param name="validate">If the data should be loaded to check the validity.</param>
        internal CompressedImage(byte[] imageData, bool validate)
        {
            this.imageData = imageData;

            if (validate)
            {
                this.GetImage();
            }
        }

        /// <summary>
        /// Gets the width (in pixels), of this image.
        /// </summary>
        public int Width
        {
            get
            {
                return this.GetImage().Width;
            }
        }

        /// <summary>
        /// Gets the height (in pixels), of this image.
        /// </summary>
        public int Height
        {
            get
            {
                return this.GetImage().Height;
            }
        }

        /// <summary>
        /// Gets the compressed image content as an <see cref="Image"/> object.
        /// </summary>
        public Image Image
        {
            get
            {
                return this.GetImage();
            }
        }

        /// <summary>
        /// Return the originally passed compressed image data as a byte array.
        /// </summary>
        /// <returns>The data originally passed to the constructor.</returns>
        public byte[] GetBytes()
        {
            // Use clone so if the caller modifies the array it doens't affect ours
            return (byte[])this.imageData.Clone();
        }

        private Image GetImage()
        {
            if (this.image == null)
            {
                using (var stream = new MemoryStream(this.imageData))
                {
                    using (var streamBitmap = new Bitmap(stream))
                    {
                        // Creating a copy of the bitmap allows us to dispose of the
                        // MemoryStream immediately instead of it needing to be kept
                        // open until the bitmap is no-longer required
                        this.image = new Bitmap(streamBitmap);
                    }
                }
            }

            return this.image;
        }
    }
}
