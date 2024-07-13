using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CommonLibCore.CommonLib
{
    public class ImageUtils
    {
        public static Image FromBase64(String Base64Image)
        {
            byte[] bytes = Convert.FromBase64String(Base64Image);
            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }
            return image;
        }

        /**
        * ThayDoiKichThuocAnh(Server.MapPath("~/hinh/tin/thumb/"), pChuoiNgayThang, 180, UpHinh.PostedFile.InputStream);
        */

        public void SaveImageRezize(
            string ImageSavePath,
            string fileName,
            int MaxWidthSideSize,
            Stream Buffer
        )
        {
            int intNewWidth;
            int intNewHeight;
            System.Drawing.Image imgInput = System.Drawing.Image.FromStream(Buffer);
            ImageCodecInfo myImageCodecInfo;
            myImageCodecInfo = GetEncoderInfo("image/jpeg");
            //
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter;
            //Giá trị width và height nguyên thủy của ảnh;
            int intOldWidth = imgInput.Width;
            int intOldHeight = imgInput.Height;

            //Kiểm tra xem ảnh ngang hay dọc;
            int intMaxSide;
            /*if (intOldWidth >= intOldHeight)
            {
            intMaxSide = intOldWidth;
            }
            else
            {
            intMaxSide = intOldHeight;
            }*/
            //Để xác định xử lý ảnh theo width hay height thì bạn bỏ note phần trên;
            //Ở đây mình chỉ sử dụng theo width nên gán luôn intMaxSide= intOldWidth; ^^;
            intMaxSide = intOldWidth;
            if (intMaxSide > MaxWidthSideSize)
            {
                //Gán width và height mới.
                double dblCoef = MaxWidthSideSize / (double)intMaxSide;
                intNewWidth = Convert.ToInt32(dblCoef * intOldWidth);
                intNewHeight = Convert.ToInt32(dblCoef * intOldHeight);
            }
            else
            {
                //Nếu kích thước width/height (intMaxSide) cũ ảnh nhỏ hơn MaxWidthSideSize thì giữ nguyên //kích thước cũ;
                intNewWidth = intOldWidth;
                intNewHeight = intOldHeight;
            }

            //Tạo một ảnh bitmap mới;
            Bitmap bmpResized = new Bitmap(imgInput, intNewWidth, intNewHeight);
            //Phần EncoderParameter cho phép bạn chỉnh chất lượng hình ảnh ở đây mình để chất lượng tốt //nhất là 100L;
            myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            //Lưu ảnh;
            bmpResized.Save(ImageSavePath + fileName, myImageCodecInfo, myEncoderParameters);

            //Giải phóng tài nguyên;

            imgInput.Dispose();
            bmpResized.Dispose();
        }

        private ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        /**
         * Crop ảnh
         * public ActionResult Upload(HttpPostedFileBase imageFile)
            {
               SaveCroppedImage(Image.FromStream(imageFile.InputStream),
               100, 100, @"C:\Temp\image.jpg");
               return View();
            }
         */
        public bool SaveCroppedImage(Image image, int maxWidth, int maxHeight, string filePath)
        {
            ImageCodecInfo jpgInfo = ImageCodecInfo
                .GetImageEncoders()
                .Where(codecInfo => codecInfo.MimeType == "image/jpeg")
                .First();
            Image finalImage = image;
            System.Drawing.Bitmap bitmap = null;
            try
            {
                int left = 0;
                int top = 0;
                int srcWidth = maxWidth;
                int srcHeight = maxHeight;
                bitmap = new System.Drawing.Bitmap(maxWidth, maxHeight);
                double croppedHeightToWidth = (double)maxHeight / maxWidth;
                double croppedWidthToHeight = (double)maxWidth / maxHeight;

                if (image.Width > image.Height)
                {
                    srcWidth = (int)(Math.Round(image.Height * croppedWidthToHeight));
                    if (srcWidth < image.Width)
                    {
                        srcHeight = image.Height;
                        left = (image.Width - srcWidth) / 2;
                    }
                    else
                    {
                        srcHeight = (int)
                            Math.Round(image.Height * ((double)image.Width / srcWidth));
                        srcWidth = image.Width;
                        top = (image.Height - srcHeight) / 2;
                    }
                }
                else
                {
                    srcHeight = (int)(Math.Round(image.Width * croppedHeightToWidth));
                    if (srcHeight < image.Height)
                    {
                        srcWidth = image.Width;
                        top = (image.Height - srcHeight) / 2;
                    }
                    else
                    {
                        srcWidth = (int)
                            Math.Round(image.Width * ((double)image.Height / srcHeight));
                        srcHeight = image.Height;
                        left = (image.Width - srcWidth) / 2;
                    }
                }
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(
                        image,
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        new Rectangle(left, top, srcWidth, srcHeight),
                        GraphicsUnit.Pixel
                    );
                }
                finalImage = bitmap;
            }
            catch { }
            try
            {
                using (EncoderParameters encParams = new EncoderParameters(1))
                {
                    encParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);
                    //quality should be in the range
                    //[0..100] .. 100 for max, 0 for min (0 best compression)
                    finalImage.Save(filePath, jpgInfo, encParams);
                    return true;
                }
            }
            catch { }
            if (bitmap != null)
            {
                bitmap.Dispose();
            }
            return false;
        }
    }
}
