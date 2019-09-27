using System;
using System.Drawing;
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
        private Bitmap InputImage2;
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
                if (InputImage != null)                                     
                {
                    InputImage2 = (Bitmap)InputImage.Clone();               // Remember the previous input
                    InputImage.Dispose();                                   // Reset image
                }
                InputImage = new Bitmap(file);                              // Create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // Dimension check
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                {
                    pictureBox1.Image = (Image)InputImage;                 // Display input image
                    pictureBox2.Image = (Image)InputImage2;
                }
            }
        }
        public void selectFuncionBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            this.resultTextBox.Visible = false;
            if (selectFunctionBox.SelectedIndex == 10)  //Move selectionbar and show a textbox when Linear filter or Thresholding are selected
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
            this.resultTextBox.Visible = false;
            if (InputImage == null) return;  // Get out if no input image
            switch (selectFunctionBox.SelectedIndex)
            {
                case 0://No selection
                    return;
                    break;
                case 7:
                    showImage(applyAND(InputImage, InputImage2));
                    break;
                case 8:
                    showImage(applyOR(InputImage, InputImage2));
                    break;
                case 9:
                    int value;
                    if (Int32.TryParse(thresholdBox.Text, out value))
                    {
                        if (value >= 0 && value < 256)
                        {
                            showImage(valueCounting(InputImage, value));
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

        private Color[,] applyAND(Bitmap InputImage, Bitmap InputImage2){
            if (OutputImage != null) OutputImage.Dispose();                              // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);     // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
            Color[,] Image2 = new Color[InputImage2.Size.Width, InputImage2.Size.Height];

            convertImageToString(Image);
            convertImageToString(Image2);
            setupProgressBar();

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image[x, y];                                             // Get the pixel color at coordinate (x,y)
                    Color pixelColor2 = Image2[x, y];
                    Color updatedColor;
                    if (pixelColor.R == 0 && pixelColor2.R == 0) { updatedColor = Color.FromArgb(0, 0, 0); }
                    else { updatedColor = Color.FromArgb(255, 255, 255); }
                    Image[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                                                  // Increment progress bar
                }

            }

            return Image;

        }
        private Color[,] applyOR(Bitmap InputImage, Bitmap InputImage2)
        {
            if (OutputImage != null) OutputImage.Dispose();                              // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);     // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
            Color[,] Image2 = new Color[InputImage2.Size.Width, InputImage2.Size.Height];

            convertImageToString(Image);
            convertImageToString(Image2);
            setupProgressBar();

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image[x, y];                                             // Get the pixel color at coordinate (x,y)
                    Color pixelColor2 = Image2[x, y];
                    Color updatedColor;
                    if ((pixelColor.R == 1 && pixelColor2.R == 0) || (pixelColor.R == 0 && pixelColor2.R == 1))  { updatedColor = Color.FromArgb(0, 0, 0); }
                    else { updatedColor = Color.FromArgb(255, 255, 255); }
                    Image[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                                                  // Increment progress bar
                }
            }

            return Image;

        }
        private Color[,] valueCounting(Bitmap InputImage, int countValue)
        {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

            convertImageToString(Image);
            setupProgressBar();

            int countedValue = 0;
            // Inversion of image
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image[x, y];                                                                     // Get the pixel color at coordinate (x,y)
                    Color updatedColor = Color.FromArgb(pixelColor.R, pixelColor.G, pixelColor.B);

                    if (pixelColor.R == countValue) { countedValue++; }                                                 // Count the set value
                    Image[x, y] = updatedColor;                                                                         // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                                                                          // Increment progress bar
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
