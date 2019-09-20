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
            if (InputImage == null) return;  // Get out if no input image
            switch (selectFunction.SelectedIndex)
            {
                case 0://No selection
                    return;
                    break;
                case 1:
                    colorInversion();
                    break;
                case 2:
                    grayscaleConversion();
                    break;
                case 3:
                    contrastAdjustment();
                    break;
                case 4:
                     linearFiltering(gaussianFilter(5,2.0));
                    break;
                case 5:
                    nonLinearFiltering();
                    break;
                case 6:
                    edgeDetection();
                    break;
                case 7:
                    thresholding();
                default:
                    return;//TODO:Add error message
            }
         
           

        }

        private void colorInversion(){
             if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

                    convertImageToString(Image);
                    setupProgressBar();

                    // Inversion of image
                    for (int x = 0; x < InputImage.Size.Width; x++){
                        for (int y = 0; y < InputImage.Size.Height; y++){
                            Color pixelColor = Image[x, y];                                                                     // Get the pixel color at coordinate (x,y)
                            Color updatedColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);    // Negative image
                            Image[x, y] = updatedColor;                                                                         // Set the new pixel color at coordinate (x,y)
                            progressBar.PerformStep();                                                                          // Increment progress bar
                        }
                    }
                    convertStringToImage(Image);
                    printImage(Image);
        }
        private void grayscaleConversion(){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    Color[,] Image2 = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

                    convertImageToString(Image2);
                    setupProgressBar();

                    // Grayscale conversion of image
                    for (int x = 0; x < InputImage.Size.Width; x++)
                    {
                        for (int y = 0; y < InputImage.Size.Height; y++)
                        {
                            Color pixelColor = Image2[x, y];                                             // Get the pixel color at coordinate (x,y)
                            var grayColor = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;            // Get average of RGB value of pixel
                            Color updatedColor = Color.FromArgb(grayColor, grayColor, grayColor);       // Set average to R, G and B values
                            Image2[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                            progressBar.PerformStep();                                                  // Increment progress bar
                        }
                    }

                    convertStringToImage(Image2);
                    printImage(Image2);
        }
        private void contrastAdjustment(){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    Color[,] Image3 = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

                    convertImageToString(Image3);
                    setupProgressBar();

                    for (int x = 0; x < InputImage.Size.Width; x++)
                    {
                        for (int y = 0; y < InputImage.Size.Height; y++)
                        {
                            Color pixelColor = Image3[x, y];// Get the pixel color at coordinate (x,y)
                            Color updatedColor = Color.FromArgb(doClampingContrast(pixelColor.R),doClampingContrast(pixelColor.G),doClampingContrast(pixelColor.B));//Increse the contrast of 50%
                            Image3[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                            progressBar.PerformStep();                                                  // Increment progress bar
                        }
                    }

                    convertStringToImage(Image3);
                    printImage(Image3);
        }
        public  double[,] gaussianFilter(int size, double sigma){
            double[,] kernel = new double[size, size];
            double kernelSum = 0;
            int center = (size - 1) / 2;
            double distance = 0;
            double constant = 1d / (2 * Math.PI * sigma * sigma);
            for (int y = -center; y <= center; y++){
                for (int x = -center; x <= center; x++){
                    distance = ((y * y) + (x * x)) / (2 * sigma * sigma);
                    kernel[y + center, x + center] = constant * Math.Exp(-distance);
                    kernelSum += kernel[y + center, x + center];
                }
            }
            for (int y = 0; y < size; y++){
                for (int x = 0; x < size; x++){
                    kernel[y, x] = kernel[y, x] * 1d / kernelSum;
                }
            }
            return kernel;
        }
        public void linearFiltering (double[,] kernel){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
               OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
               Color[,] Image2 = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

            convertImageToString(Image2);
            setupProgressBar();
            int size =(int)( 5/2); //problem in retrieving size of kernel. TODO: create  obj Kernel thata contains size of kernel

            for (var x = size; x < InputImage.Size.Width - size; x++)
            {
                for (var y = size; y < InputImage.Size.Height - size; y++)
                {
                    int r = 0, b = 0, g = 0;

                    for (var i = 0; i < 5; i++)
                    {
                        for (var j = 0; j < 5; j++)
                        {   //Convolution
                            var temp = InputImage.GetPixel(x + i - size, y + j - size);

                            r +=(int) (kernel[i, j] * temp.R);
                            g += (int) (kernel[i, j] * temp.G);
                            b +=(int) (kernel[i, j] * temp.B);
                        }
                    }
                    Color updatedColor = Color.FromArgb(r, g, b);       // Set average to R, G and B values
                    Image2[x, y] = updatedColor;                        // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                                                  // Increment progress bar
                            
                }
            }


            //do actual things

             convertStringToImage(Image2);
             printImage(Image2);


        
        }
        //Utilities

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }
        private int doClampingContrast(double color){ //adds contrast of 50% (1.5) to the image using clamping
            if(color*1.5 <= 255 && color*1.5 >=0)
               return (int) (color*1.5);
             else if (color*1.5 > 255)
               return 255;
             else
               return 0;
        }
        private void setupProgressBar()
            {
                // Setup progress bar
                progressBar.Visible = true;
                progressBar.Minimum = 1;
                progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
                progressBar.Value = 1;
                progressBar.Step = 1;
            }
        private void convertImageToString(Color[,] Image)
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
        private void convertStringToImage(Color[,] Image)
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
        private  void printImage(Color[,] Image)
            {
                pictureBox2.Image = (Image)OutputImage;                         // Display output image
                progressBar.Visible = false;                                    // Hide progress bar
            }

    }
}
