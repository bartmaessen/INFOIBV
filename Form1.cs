using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;

        public INFOIBV()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
           if (openImageDialog.ShowDialog() == DialogResult.OK)             // Open File Dialog
            {
                string file = openImageDialog.FileName;                     // Get the file name
                imageFileName.Text = file;                                  // Show file name
                if (InputImage != null) InputImage.Dispose();               // Reset image
                InputImage = new Bitmap(file);                              // Create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // Dimension check
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                    pictureBox1.Image = (Image) InputImage;                 // Display input image
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // Get out if no input image
            if (selectFunction.SelectedIndex == 0) return;                  // Get out if no function selection
            if (selectFunction.SelectedIndex == 1)
            {
                if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

                convertImageToString(Image);
                setupProgressBar();
                              
                // Inversion of image
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    for (int y = 0; y < InputImage.Size.Height; y++)
                    {
                        Color pixelColor = Image[x, y];                                                                     // Get the pixel color at coordinate (x,y)
                        Color updatedColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);    // Negative image
                        Image[x, y] = updatedColor;                                                                         // Set the new pixel color at coordinate (x,y)
                        progressBar.PerformStep();                                                                          // Increment progress bar
                    }
                }

                convertStringToImage(Image);
                printImage(Image);
            }

            if (selectFunction.SelectedIndex == 2)
            {
                if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

                convertImageToString(Image);
                setupProgressBar();

                // Grayscale conversion of image
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    for (int y = 0; y < InputImage.Size.Height; y++)
                    {
                        Color pixelColor = Image[x, y];                                             // Get the pixel color at coordinate (x,y)
                        var grayColor = (pixelColor.R + pixelColor.G + pixelColor.B)/ 3;            // Get average of RGB value of pixel
                        Color updatedColor = Color.FromArgb(grayColor, grayColor, grayColor);       // Set average to R, G and B values
                        Image[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                        progressBar.PerformStep();                                                  // Increment progress bar
                    }
                }

                convertStringToImage(Image);
                printImage(Image);
            }

            void setupProgressBar()
            {
                // Setup progress bar
                progressBar.Visible = true;
                progressBar.Minimum = 1;
                progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
                progressBar.Value = 1;
                progressBar.Step = 1;
            }

            void convertImageToString(Color[,] Image)
            {
                // Copy input Bitmap to array            
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    for (int y = 0; y < InputImage.Size.Height; y++)
                    {
                        Image[x, y] = InputImage.GetPixel(x, y);                // Set pixel color in array at (x,y)
                    }
                }
            }

            void convertStringToImage(Color[,] Image)
            {
                // Copy array to output Bitmap
                for (int x = 0; x < InputImage.Size.Width; x++)
                {
                    for (int y = 0; y < InputImage.Size.Height; y++)
                    {
                        OutputImage.SetPixel(x, y, Image[x, y]);               // Set the pixel color at coordinate (x,y)
                    }
                }
            }

            void printImage(Color[,] Image)
            {
                pictureBox2.Image = (Image)OutputImage;                         // Display output image
                progressBar.Visible = false;                                    // Hide progress bar
            }
        }
        
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }

    }
}
