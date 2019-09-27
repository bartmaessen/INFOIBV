using System;
using System.Drawing;
using System.Windows.Forms;

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
            if (selectFunctionBox.SelectedIndex == 7 || selectFunctionBox.SelectedIndex == 4 || selectFunctionBox.SelectedIndex == 16)  //Move selectionbar and show a textbox when Linear filter or Thresholding are selected
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
            this.resultTextBox.Visible = true;
            this.resultTextBox.Text = countedValue.ToString();

            return Image;
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
