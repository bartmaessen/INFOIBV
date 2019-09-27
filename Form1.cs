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
/* 
 * By Bart Maessen 4033620 & Teddy Gyabaah 6879136
 */
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
        public class Kernel{
            double[,] matrix;
            int size;
            public Kernel(double[,] matrix, int size){
                this.matrix = matrix;
                this.size = size;
            }

            public double[,] getMatrix (){
                return this.matrix;
            }
            public int getSize(){
                return this.size;
            }
        }
        public class StructElement{
            int[,] matrix;
            int size;
            public StructElement(int[,] matrix, int size){
                this.matrix = matrix;
                this.size = size;
            }

            public int[,] getMatrix (){
                return this.matrix;
            }
            public int getSize(){
                return this.size;
            }
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
                        int size =11;//Gaussian kernel size
                        showImage(linearFiltering(InputImage,gaussianFilter(size,sigma)));
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
                    showImage(edgeDetection(InputImage,new Kernel(new double[,]{{ -1, 0, 1 }, //Edge detection using sobel operator
                                                                     { -2, 0, 2 },
                                                                     { -1, 0, 1 }},3),
                                                        new Kernel(new double[,] { {  1,  2,  1 },
                                                                        {  0,  0,  0 }, 
                                                                        { -1, -2, -1 }},3)));
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
                case 8:
                    Color[,] Image1 = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
                    convertImageToString(Image1);
                    showImage(erosionGray(Image1,structuringElementGrayscale('+',5)));
                    break;
                case 9:
                    //showImage(dilatation(InputImage,structuringElementGrayscale('+',5)));
                    Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
                    convertImageToString(Image);
                    showImage(dilatationGray(Image,structuringElementGrayscale('+',5)));
                    break;
                default:
                    return;
            }
        }
        
        //Assignment1
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
        private Kernel gaussianFilter(int size, double sigma){
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

            return new Kernel(kernel,size);
        }
        private Color[,] linearFiltering (Bitmap InputImage, Kernel kernell){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
               OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
               Color[,] Image2 = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

            convertImageToString(Image2);
            setupProgressBar();
            double[,] kernel = kernell.getMatrix();
            int size =(int)(kernell.getSize()/2);

            for (var x = size; x < InputImage.Size.Width - size; x++)
            {
                for (var y = size; y < InputImage.Size.Height - size; y++)
                {
                    int r = 0, b = 0, g = 0;

                    for (var i = 0; i < kernell.getSize(); i++)
                    {
                        for (var j = 0; j < kernell.getSize(); j++)
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
                    Image2[x, y] = updatedColor;// Set the new pixel color at coordinate (x,y)
                     progressBar.PerformStep();
                            
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
        private Color[,] edgeDetection(Bitmap inputImage,Kernel edgeKernelX,Kernel edgekernelY){
            Color[,] outputImage = new Color[InputImage.Size.Width, InputImage.Size.Height];
            Color[,] dx = linearFiltering(InputImage,edgeKernelX) ;  
            Color[,] dy= linearFiltering(InputImage,edgekernelY);

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

        //Assignment2
        private StructElement structuringElementBinary(char shape, int size){
            int[,] output = new int[size,size];
            if(shape == '+'){
                for(int i=0; i<size;i++){
                   for(int j=0;j<size;j++){
                        if((i==0 && j==0)||(i==0 && j==size-1) || (i==size-1 && j==0) || (i==size-1 && j== size-1))
                            output[i,j] = 0;
                        else
                            output[i,j] = 1;
                    }
                }
            }
            else if(shape == 'r'){                                  //'r' stands for rectangle
              for(int i=0;i<size;i++){
                 for(int j=0;j<size;j++){
                        output[i,j]=1;
                  }
              }
            }
            return new StructElement(output,size);
        }
        private StructElement structuringElementGrayscale(char shape, int size){
            int[,] output = new int[size,size];
            if(shape == '+'){
                for(int i=0; i<size;i++){
                   for(int j=0;j<size;j++){
                        if((i==0 && j==0)||(i==0 && j==size-1) || (i==size-1 && j==0) || (i==size-1 && j== size-1))
                            output[i,j] = -256;
                        else
                            output[i,j] = 0;
                    }
                }
            }
            else if(shape == 'r'){                                  //'r' stands for rectangle
              for(int i=0;i<size;i++){
                 for(int j=0;j<size;j++){
                        output[i,j]=0;
                  }
              }
            }
            return new StructElement(output,size);
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
        private Color[,] erosion(Bitmap InputImage,StructElement structElement){// To adjust
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
                    Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height]; 
                    int[,] structMatrix = structElement.getMatrix();
                    
                    convertImageToString(Image);
                    setupProgressBar();
                    if(isBinary(Image,InputImage.Size.Width,InputImage.Size.Height)){
                        output = erosionBinary(Image,structElement);
                    }else{
                        output = erosionGray(Image,structElement);
                    }                                                                                       
                    return output;

        }
        private Color[,] dilatationBinary(Color[,] Image,StructElement structElement){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    //Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
                    Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];
                    //convertImageToString(Image);
                    setupProgressBar();
                    int[,] H = structElement.getMatrix();
                    int Hsize = structElement.getSize();
                    for(int i =0;i<InputImage.Size.Width;i++){
                        for(int j=0;j<InputImage.Size.Height;j++){
                            output[i,j] = Color.FromArgb(255,255,255);
                        }
                    }
                   for(int i=0;i<Hsize;i++){
                        for(int j=0;j<Hsize;j++){
                         if(H[i,j] == 1){
                          for(int u =0;u< InputImage.Size.Width-Hsize;u++){
                             for(int v=0;v<InputImage.Size.Height-Hsize;v++){
                              if(Image[u,v].R == 0){
                                output[u+i,v+j]= Color.FromArgb(0,0,0);
                                
                              }
                             }
                            }
                         }
                        }
                    }

            return output;
                    
        }
        private Color[,] erosionBinary(Color[,] Image,StructElement structElement){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    //Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
                    Color[,] output = colorInversion(InputImage);
                    //convertImageToString(Image);
                    setupProgressBar();

                    return myColorInversion(dilatationBinary(output,structElement));

        }
        private Color[,] erosionGray(Color[,] Image,StructElement structElement){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    //Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
                    Color[,] output = colorInversion(InputImage);
                    //convertImageToString(Image);
                    setupProgressBar();

                    return myColorInversion(dilatationGray(output,structElement));

        }
        private Color[,] dilatationGray(Color[,]Image,StructElement structElement){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    //Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
                    Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height]; 
                    int[,] structMatrix = structElement.getMatrix();

                    for(int i =0;i<InputImage.Size.Width;i++){
                        for(int j=0;j<InputImage.Size.Height;j++){
                            output[i,j] = Color.FromArgb(255,255,255);
                        }
                    }
                    int offset = (structElement.getSize()-1)/2;
                    //convertImageToString(Image);
                    setupProgressBar();
                        int value=0;
                        for(int u=offset;u<InputImage.Size.Width-offset;u++){
                            for(int v=offset;v<InputImage.Size.Height-offset;v++){
                                value = 0;
                                    for(int i=-offset; i<offset;i++){
                                        for(int j=-offset; j<offset;j++){
                                          if(structMatrix[i+offset,j+offset] != -256){        //cell off
                                           if(Image[u+i,v+j].R+structMatrix[i+offset,j+offset] > 255)
                                              value = Math.Max(value, 255);
                                           else
                                              value= Math.Max(value, (Image[u+i,v+j].R)+(structMatrix[i+offset,j+offset]));
                                           }
                                        }
                                    }
                                output[u,v]= Color.FromArgb(value,value,value);
                                progressBar.PerformStep();
                               
                                
                            }
                        } 
                    

                    return output;
        }
        private Color[,] dilatation(Bitmap InputImage,StructElement structElement){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
                    Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height]; 
                    int[,] structMatrix = structElement.getMatrix();
                    
                    convertImageToString(Image);
                    setupProgressBar();
                    if(isBinary(Image,InputImage.Size.Width,InputImage.Size.Height)){
                        output = dilatationBinary(Image,structElement);
                    }else{
                        output = dilatationGray(Image,structElement);
                    }                                                                                       
                    return output;
        }
        private Color[,] opening(Color[,]Image,StructElement structElement){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

                    //convertImageToString(Image);
                    setupProgressBar();
                    if(isBinary(Image,InputImage.Size.Width,InputImage.Size.Height)){
                        output = erosionBinary(Image,structElement);
                        output = dilatationBinary(output,structElement);
                    }
                    else{
                        output = erosionGray(Image,structElement);
                        output = dilatationGray(output,structElement);
                    }
                    
                    return output;
        }
        private Color[,] closing(Color[,]Image,StructElement structElement){
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                    Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

                    //convertImageToString(Image);
                    setupProgressBar();
                    if(isBinary(Image,InputImage.Size.Width,InputImage.Size.Height)){
                        output = dilatationBinary(Image,structElement);
                        output = erosionBinary(output,structElement);
                    }
                    else{
                        output = dilatationGray(Image,structElement);
                        output = erosionGray(output,structElement);
                    }
                    
                    return output;
        }
        private Color[,] myColorInversion(Color[,] Image){
             if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
                    OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
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

        private bool isBinary(Color[,] image,int height,int width){
            for(int i=0; i<height;i++){
                for(int j=0;j<width;j++){
                    if(image[i,j].R != 255 && image[i,j].R != 0 ){
                        return false;
                    }
                }
            }
            return true;
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
