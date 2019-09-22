using System;
using System.Collections;
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
            if (selectFunctionBox.SelectedIndex == 7 || selectFunctionBox.SelectedIndex == 4)  //Move selectionbar and show a textbox when Linear filter or Thresholding are selected
                {
                this.selectFunctionBox.Location = new System.Drawing.Point(357, 13);
                this.thresholdBox.Visible = true;
            }
            else
            {
                this.selectFunctionBox.Location = new System.Drawing.Point(402, 13);        //Move selectionbar and hide textbox when others are selected
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
                    showImage(colorInversion(InputImage));
                    break;
                case 2:
                    showImage(grayscaleConversion(InputImage));               
                    break;
                case 3:
                    showImage(contrastAdjustment(InputImage));
                    break;
                case 4:
                    int sigma;
                    if (Int32.TryParse(thresholdBox.Text, out sigma))
                    {
                        showImage(linearFiltering(InputImage, gaussianFilter(3, sigma)));
                    }
                    else
                    {
                        MessageBox.Show("Select an integer value for sigma");
                    }
                    break;
                case 5:
                    showImage(nonLinearFiltering(InputImage,3));
                    break;
                case 6:
                    showImage(edgeDetection(InputImage,new double[,]{{ -1, 0, 1 }, //Edge detection using sobel operator
                                                                     { -2, 0, 2 },
                                                                     { -1, 0, 1 }},
                                                        new double[,] { {  1,  2,  1 },
                                                                        {  0,  0,  0 }, 
                                                                        { -1, -2, -1 }}));
                    break;
                case 7:
                    int ath;
                    if (Int32.TryParse(thresholdBox.Text, out ath))
                    {
                        if (ath >= 0 && ath < 255)
                        {
                           showImage(thresholding(ath));
                        }
                    }
                    else
                    {
                        MessageBox.Show("Insert an integer from 0 untill 255");
                    }
                    break;
                default:
                    return;
            }
        }

        private Color[,] colorInversion(Bitmap InputImage){
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

                    return Image;
        }
        private Color[,] grayscaleConversion(Bitmap InputImage){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    Color[,] Image2 = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

                    convertImageToString(Image2);
                    setupProgressBar();
                    
                    return doGrayscale(Image2, true);
        }
        private Color[,] doGrayscale(Color[,] image, bool isFinalOperation){
                     // Grayscale conversion of image
                    for (int x = 0; x < InputImage.Size.Width; x++)
                    {
                        for (int y = 0; y < InputImage.Size.Height; y++)
                        {
                            Color pixelColor = image[x, y];                                             // Get the pixel color at coordinate (x,y)
                            var grayColor = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;            // Get average of RGB value of pixel
                            Color updatedColor = Color.FromArgb(grayColor, grayColor, grayColor);        // Set average to R, G and B values
                            image[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                            if (isFinalOperation == true) progressBar.PerformStep();                     // Increment progress bar if Grayscale is used as final operation
                        }
                    }
                    return image;
        }
        private Color[,] contrastAdjustment(Bitmap InputImage){
            if (OutputImage != null) OutputImage.Dispose();                                      // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);     // Create new output image
            Color[,] image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

            convertImageToString(image);
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
                    if (image[x,y].R > aRhigh) aRhigh = image[x,y].R; // Determine the ahigh an alow values of the R,G,B channels
                    if (image[x,y].R < aRlow) aRlow = image[x,y].R;
                    if (image[x,y].G > aGhigh) aGhigh = image[x,y].G;
                    if (image[x,y].G < aGlow) aGlow = image[x,y].G;
                    if (image[x,y].B > aBhigh) aBhigh = image[x,y].B;
                    if (image[x,y].B < aBlow) aBlow = image[x,y].B;
                }
            }

            var amin = 0;
            var amax = 255;
            for (int x = 0; x < InputImage.Size.Width; x++) 
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = image[x, y];// Get the pixel color at coordinate (x,y)
                    var r = amin + (amax - amin)/(aRhigh - aRlow)*(image[x,y].R-aRlow); // Perform contrast adjustment for each channel 
                    var g = amin + (amax - amin)/(aGhigh - aGlow)*(image[x,y].G-aGlow);
                    var b = amin + (amax - amin)/(aBhigh - aBlow)*(image[x,y].B-aBlow);
                    Color updatedColor = Color.FromArgb(r,g,b);
                    image[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                                                  // Increment progress bar
                }
            }
            return image;
            //convertStringToImage(image);
            //printImage(image);
        }
        private double[,] gaussianFilter(int size, double sigma){
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
        private Color[,] linearFiltering (Bitmap InputImage,double[,] kernel){
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

                    for (var i = 0; i < 3; i++)
                    {
                        for (var j = 0; j < 3; j++)
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

            return Image2;
            // convertStringToImage(Image2);
             //printImage(Image2);
        }
        private Color[,] linearFiltering2 (double[,] kernel){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
               OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
               Color[,] Image2 = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

            convertImageToString(Image2);
           // setupProgressBar();
            int size =(int)( 3/2); //problem in retrieving size of kernel. TODO: create  obj Kernel thata contains size of kernel

            for (var x = size; x < InputImage.Size.Width - size; x++)
            {
                for (var y = size; y < InputImage.Size.Height - size; y++)
                {
                    int r = 0, b = 0, g = 0;

                    for (var i = 0; i < 3; i++)
                    {
                        for (var j = 0; j < 3; j++)
                        {   //Convolution
                            var temp = InputImage.GetPixel(x + i - size, y + j - size);

                            r +=(int) (kernel[i, j] * temp.R);
                            g += (int) (kernel[i, j] * temp.G);
                            b +=(int) (kernel[i, j] * temp.B);

                            if(r> 255)
                                r=255;
                            else if(r<0)
                                r=0;
                            if(g> 255)
                                g=255;
                            else if(g<0)
                                g=0;
                            if(b> 255)
                                b=255;
                            else if(b<0)
                                b=0;
                        }
                    }
                    Color updatedColor = Color.FromArgb(r, g, b);       // Set average to R, G and B values
                    Image2[x, y] = updatedColor;                        // Set the new pixel color at coordinate (x,y)
                            
                }
            }
            return Image2;
        
        }
        private Color[,] thresholding(int ath){
            if (OutputImage != null) OutputImage.Dispose();                              // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);     // Create new output image
            Color[,] Image5 = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

            convertImageToString(Image5);
            setupProgressBar();

            Image5 = doGrayscale(Image5, false);

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image5[x, y];                                             // Get the pixel color at coordinate (x,y)
                    int newColor;
                    if (pixelColor.R < ath) { newColor = 0; }                                    // Set new color value according to the threshold value
                    else { newColor = 255; }
                    Color updatedColor = Color.FromArgb(newColor, newColor, newColor);           // Make the new color value into a 3 channel color array
                    Image5[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                                                   // Increment progress bar
                }

            }

             return Image5;
        
        }
        private Color[,] nonLinearFiltering(Bitmap InputImage, int medianSize){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

                    convertImageToString(Image);
                    setupProgressBar();
            List<int> neighbourPixelsR = new List<int>();
            //List<int> neighbourPixelsG = new List<int>();
            //List<int> neighbourPixelsB = new List<int>();
            int filterOffset = (medianSize - 1) / 2;

            for (int offsetY = filterOffset; offsetY < InputImage.Size.Height - filterOffset ; offsetY++){
                for (int offsetX = filterOffset; offsetX < InputImage.Size.Width - filterOffset; offsetX++){
                    
                    neighbourPixelsR.Clear();
                   // neighbourPixelsG.Clear();
                   //neighbourPixelsB.Clear();

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++){
                            for (int filterX = -filterOffset;filterX <= filterOffset; filterX++){
                                 neighbourPixelsR.Add(InputImage.GetPixel(offsetX-filterX,offsetY-filterY).ToArgb());
               }
           }
            neighbourPixelsR.Sort();//Using only the Red channel to choose the median pixel
            Image[offsetX,offsetY] = Color.FromArgb(neighbourPixelsR.ElementAt((int)((neighbourPixelsR.Count)/ 2)));
             progressBar.PerformStep();   
                }        
            }

            return Image;
                    
        }
        private Color[,] edgeDetection(Bitmap inputImage,double[,] edgeKernelX,double[,] edgekernelY){
            Color[,] outputImage = new Color[InputImage.Size.Width, InputImage.Size.Height];
            Color[,] dx = linearFiltering2(edgeKernelX) ;  
            Color[,] dy= linearFiltering2(edgekernelY);

            setupProgressBar();

            for (int x = 0; x < InputImage.Size.Width; x++) 
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    //total rgb values for this pixel
                    int rt =(int) (Math.Sqrt((dx[x,y].R * dx[x,y].R) + (dy[x,y].R * dy[x,y].R)));
                    int gt =(int) (Math.Sqrt((dx[x,y].G * dx[x,y].G) + (dy[x,y].G * dy[x,y].G)));
                    int bt =(int) (Math.Sqrt((dx[x,y].R * dx[x,y].B) + (dy[x,y].B * dy[x,y].B)));

                    if(rt> 255)
                        rt=255;
                    else if(rt<0)
                        rt=0;
                    if(gt> 255)
                        gt=255;
                    else if(gt<0)
                        gt=0;
                    if(bt> 255)
                        bt=255;
                    else if(bt<0)
                        bt=0;
                    outputImage[x,y]= Color.FromArgb(rt,gt,bt);
                    progressBar.PerformStep(); 

                }
            }

            return outputImage;
       
        }
 
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
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
        private void showImage(Color[,] image){
            convertStringToImage(image);
            printImage(image);
        }

    }
}
