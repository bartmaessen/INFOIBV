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
        public void selectFuncionBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (selectFunctionBox.SelectedIndex == 7)
                {
                this.selectFunctionBox.Location = new System.Drawing.Point(357, 13);
                this.thresholdBox.Visible = true;
            }
            else
            {
                this.selectFunctionBox.Location = new System.Drawing.Point(402, 13);
                this.thresholdBox.Visible = false;
            }
        }
            private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;  // Get out if no input image
            switch (selectFunctionBox.SelectedIndex)
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
                    int ath;
                    if (Int32.TryParse(thresholdBox.Text, out ath))
                    {
                        if (ath >= 0 && ath < 255)
                        {
                            thresholding(ath);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Insert an integer from 0 untill 255");
                    }
                    break;
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
                    
                    doGrayscale(Image2, true);

                    convertStringToImage(Image2);
                    printImage(Image2);
        }
        private void doGrayscale(Color[,] Image2, bool isFinalOperation){
                     // Grayscale conversion of image
                    for (int x = 0; x < InputImage.Size.Width; x++)
                    {
                        for (int y = 0; y < InputImage.Size.Height; y++)
                        {
                            Color pixelColor = Image2[x, y];                                             // Get the pixel color at coordinate (x,y)
                            var grayColor = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;            // Get average of RGB value of pixel
                            Color updatedColor = Color.FromArgb(grayColor, grayColor, grayColor);        // Set average to R, G and B values
                            Image2[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                            if (isFinalOperation == true) progressBar.PerformStep();                     // Increment progress bar if Grayscale is used as final operation
                        }
                    }
        }
        private void contrastAdjustment(){
            if (OutputImage != null) OutputImage.Dispose();                                      // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);     // Create new output image
            Color[,] Image3 = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

            convertImageToString(Image3);
            setupProgressBar();

            var aRhigh = 0;
            var aRlow = 255;
            var aGhigh = 0;
            var aGlow = 255;
            var aBhigh = 0;
            var aBlow = 255;
            for (int x = 0; x < InputImage.Size.Width; x++) 
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    if (Image3[x,y].R > aRhigh) aRhigh = Image3[x,y].R; // Determine the ahigh an alow values of the R,G,B channels
                    if (Image3[x,y].R < aRlow) aRlow = Image3[x,y].R;
                    if (Image3[x,y].G > aGhigh) aGhigh = Image3[x,y].G;
                    if (Image3[x,y].G < aGlow) aGlow = Image3[x,y].G;
                    if (Image3[x,y].B > aBhigh) aBhigh = Image3[x,y].B;
                    if (Image3[x,y].B < aBlow) aBlow = Image3[x,y].B;
                }
            }

            var amin = 0;
            var amax = 255;
            for (int x = 0; x < InputImage.Size.Width; x++) 
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image3[x, y];// Get the pixel color at coordinate (x,y)
                    var r = amin + (amax - amin)/(aRhigh - aRlow)*(Image3[x,y].R-aRlow); // Perform contrast adjustment for each channel 
                    var g = amin + (amax - amin)/(aGhigh - aGlow)*(Image3[x,y].G-aGlow);
                    var b = amin + (amax - amin)/(aBhigh - aBlow)*(Image3[x,y].B-aBlow);
                    Color updatedColor = Color.FromArgb(r,g,b);
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
        public void thresholding(int ath){
            if (OutputImage != null) OutputImage.Dispose();                                      // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);     // Create new output image
            Color[,] Image5 = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

            convertImageToString(Image5);
            setupProgressBar();

            doGrayscale(Image5, false);
            
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image5[x, y];                                             // Get the pixel color at coordinate (x,y)
                    int newColor;
                    if (Image5[x, y].R < ath) newColor = 0;                                      // Set new color value according to the threshold value
                    else newColor = 255;
                    Color updatedColor = Color.FromArgb(newColor, newColor, newColor);           // Make the new color value into a 3 channel color array
                    Image5[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                                                   // Increment progress bar
                }
                
            }
            convertStringToImage(Image5);
            printImage(Image5);
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
