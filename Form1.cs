using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;
/* 
 * By Bart Maessen 4033620 & Teddy Gyabaah 6879136
 */
namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage = null;
        private Bitmap InputImage2 = null;
        private Bitmap OutputImage;

        public INFOIBV()
        {
            InitializeComponent();
        }
        public class Kernel {
            double[,] matrix;
            int size;
            public Kernel(double[,] matrix, int size) {
                this.matrix = matrix;
                this.size = size;
            }

            public double[,] getMatrix() {
                return this.matrix;
            }
            public int getSize() {
                return this.size;
            }
        }
        public class StructElement {
            int[,] matrix;
            int size;
            public StructElement(int[,] matrix, int size) {
                this.matrix = matrix;
                this.size = size;
            }

            public int[,] getMatrix() {
                return this.matrix;
            }
            public int getSize() {
                return this.size;
            }
        }
        public class HCounting {
            Bitmap histogram;
            int noValue;
            public HCounting(Bitmap histogram, int noValue) {
                this.histogram = histogram;
                this.noValue = noValue;
            }

            public Bitmap getHistogram() {
                return this.histogram;
            }
            public int getValueCounting() {
                return this.noValue;
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
                }
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
                case 1:
                    showImage(erosion(InputImage, structuringElementGrayscale('+', 9), null));
                    break;
                case 2:
                    showImage(dilatation(InputImage, structuringElementGrayscale('+', 11), null));
                    break;
                case 3:
                    showImage(opening(InputImage, structuringElementBinary('+', 43)));
                    break;
                case 4:
                    showImage(closing(InputImage, structuringElementBinary('+', 3)));
                    break;
                case 5:
                    showImage(colorInversion(InputImage));
                    break;
                case 6:
                    if (InputImage2 == null)
                        MessageBox.Show("Insert a second image for this operation");
                    else
                        showImage(applyAND(InputImage, InputImage2));
                    break;
                case 7:
                    if (InputImage2 == null)
                        MessageBox.Show("Insert a second image for this operation");
                    else
                        showImage(applyOR(InputImage, InputImage2));
                    break;
                case 8:
                    HCounting result = valueCounting(InputImage);
                    pictureBox2.Image = result.getHistogram();
                    break;
                case 9:
                     richTextBox1.Text =displayMembers(boundaryTrace(InputImage));
                    break;
                default:
                    return;
            }
        }

        private Color[,] doGrayscale(Bitmap InputImage) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] image = new Color[InputImage.Size.Width, InputImage.Size.Height];


            convertImageToString(image);
            // Grayscale conversion of image
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = image[x, y];                                             // Get the pixel color at coordinate (x,y)
                    var grayColor = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;            // Get average of RGB value of pixel
                    Color updatedColor = Color.FromArgb(grayColor, grayColor, grayColor);       // Set average to R, G and B values
                    image[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                                                  // Increment progress bar
                }
            }
            return image;

        }

        //Assignment2
        private StructElement structuringElementBinary(char shape, int size) {
            int[,] output = new int[size, size];
            if (shape == '+') {
                for (int i = 0; i < size; i++) {
                    for (int j = 0; j < size; j++) {
                        if ((i == 0 && j == 0) || (i == 0 && j == size - 1) || (i == size - 1 && j == 0) || (i == size - 1 && j == size - 1))
                            output[i, j] = 0;
                        else
                            output[i, j] = 1;
                    }
                }
            }
            else if (shape == 'r') {                                  //'r' stands for rectangle
                for (int i = 0; i < size; i++) {
                    for (int j = 0; j < size; j++) {
                        output[i, j] = 1;
                    }
                }
            }
            return new StructElement(output, size);
        }
        private StructElement structuringElementGrayscale(char shape, int size) {
            int[,] output = new int[size, size];
            if (shape == '+') {
                for (int i = 0; i < size; i++) {
                    for (int j = 0; j < size; j++) {
                        if ((i == 0 && j == 0) || (i == 0 && j == size - 1) || (i == size - 1 && j == 0) || (i == size - 1 && j == size - 1))
                            output[i, j] = -256;
                        else
                            output[i, j] = 0;
                    }
                }
            }
            else if (shape == 'r') {                                  //'r' stands for rectangle
                for (int i = 0; i < size; i++) {
                    for (int j = 0; j < size; j++) {
                        output[i, j] = 0;
                    }
                }
            }
            return new StructElement(output, size);
        }
        private Color[,] colorInversion(Bitmap InputImage) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

            convertImageToString(Image);
            setupProgressBar();

            // Inversion of image
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
                    Color pixelColor = Image[x, y];                                                                     // Get the pixel color at coordinate (x,y)
                    Color updatedColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);    // Negative image
                    Image[x, y] = updatedColor;                                                                         // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                                                                          // Increment progress bar
                }
            }

            return Image;
        }
        private Color[,] erosion(Bitmap InputImage, StructElement structElement, Bitmap controlImage) {// To adjust
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
            Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];
            int[,] structMatrix = structElement.getMatrix();

            convertImageToString(Image);
            setupProgressBar();

            if (isBinary(Image, InputImage.Size.Width, InputImage.Size.Height)) {
                MessageBox.Show("Binary!");
                if (controlImage == null) {
                    output = erosionBinary(Image, structElement);
                    MessageBox.Show("Binary Erosion!");
                } else
                    output = geodesicErosionBinary(Image, structElement, controlImage);
            } else {
                if (controlImage == null)
                    output = erosionGray(Image, structElement);
                else
                    output = geodesicErosionGray(Image, structElement, controlImage);
            }
            return output;

        }
        private Color[,] geodesicErosionBinary(Color[,] Image, StructElement structElement, Bitmap controlImage) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] controlImagee = new Color[InputImage2.Size.Width, InputImage2.Size.Height];
            int[,] structMatrix = structElement.getMatrix();
            Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];

            convertImageToString2(controlImagee);

            output = dilatationBinary(Image, structElement);
            output = myApplyOR(output, controlImagee);

            return output;
        }
        private Color[,] geodesicDilatationBinary(Color[,] Image, StructElement structElement, Bitmap controlImage) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] controlImagee = new Color[InputImage2.Size.Width, InputImage2.Size.Height];
            int[,] structMatrix = structElement.getMatrix();
            Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];

            convertImageToString2(controlImagee);

            output = dilatationBinary(Image, structElement);
            output = myApplyAND(output, controlImagee);

            return output;
        }
        private Color[,] geodesicErosionGray(Color[,] Image, StructElement structElement, Bitmap controlImage) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] controlImagee = new Color[InputImage2.Size.Width, InputImage2.Size.Height];
            int[,] structMatrix = structElement.getMatrix();
            Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];

            convertImageToString2(controlImagee);

            output = erosionGray(Image, structElement);
            output = myApplyMax(output, controlImagee);

            return output;
        }
        private Color[,] geodesicDilatationGray(Color[,] Image, StructElement structElement, Bitmap controlImage) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] controlImagee = new Color[InputImage2.Size.Width, InputImage2.Size.Height];
            int[,] structMatrix = structElement.getMatrix();
            Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];

            convertImageToString2(controlImagee);

            output = dilatationGray(Image, structElement);
            output = myApplyMin(output, controlImagee);

            return output;
        }
        private Color[,] dilatationBinary(Color[,] Image, StructElement structElement) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];
            setupProgressBar();
            int[,] H = structElement.getMatrix();
            int Hsize = structElement.getSize();

            for (int i = 0; i < InputImage.Size.Width; i++) {
                for (int j = 0; j < InputImage.Size.Height; j++) {
                    output[i, j] = Color.FromArgb(255, 255, 255);
                }
            }
            for (int i = 0; i < Hsize; i++) {
                for (int j = 0; j < Hsize; j++) {
                    if (H[i, j] == 1) {
                        for (int u = 0; u < InputImage.Size.Width - Hsize; u++) {
                            for (int v = 0; v < InputImage.Size.Height - Hsize; v++) {
                                if (Image[u, v].R == 0) {
                                    output[u + i, v + j] = Color.FromArgb(0, 0, 0);

                                }
                            }
                        }
                    }
                }
            }

            return output;

        }
        private Color[,] erosionBinary(Color[,] Image, StructElement structElement) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] output = colorInversion(InputImage);

            setupProgressBar();

            return myColorInversion(dilatationBinary(output, structElement));

        }
        private Color[,] erosionGray(Color[,] Image, StructElement structElement) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] output = myColorInversion(Image);
            setupProgressBar();

            return myColorInversion(dilatationGray(output, structElement));

        }
        private Color[,] dilatationGray(Color[,] Image, StructElement structElement) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
                                                                                        //Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
            Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];
            int[,] structMatrix = structElement.getMatrix();

            for (int i = 0; i < InputImage.Size.Width; i++) {
                for (int j = 0; j < InputImage.Size.Height; j++) {
                    output[i, j] = Color.FromArgb(255, 255, 255);
                }
            }
            int offset = (structElement.getSize() - 1) / 2;
            //convertImageToString(Image);
            setupProgressBar();
            int value = 0;
            for (int u = offset; u < InputImage.Size.Width - offset; u++) {
                for (int v = offset; v < InputImage.Size.Height - offset; v++) {
                    value = 0;
                    for (int i = -offset; i < offset; i++) {
                        for (int j = -offset; j < offset; j++) {
                            if (structMatrix[i + offset, j + offset] != -256) {        //cell off
                                if (Image[u + i, v + j].R + structMatrix[i + offset, j + offset] > 255)
                                    value = Math.Max(value, 255);
                                else
                                    value = Math.Max(value, (Image[u + i, v + j].R) + (structMatrix[i + offset, j + offset]));
                            }
                        }
                    }
                    output[u, v] = Color.FromArgb(value, value, value);
                    progressBar.PerformStep();


                }
            }


            return output;
        }
        private Color[,] dilatation(Bitmap InputImage, StructElement structElement, Bitmap controlImage) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
            Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];
            int[,] structMatrix = structElement.getMatrix();

            convertImageToString(Image);
            setupProgressBar();
            if (isBinary(Image, InputImage.Size.Width, InputImage.Size.Height)) {
                if (controlImage == null)
                    output = dilatationBinary(Image, structElement);
                else
                    output = geodesicDilatationBinary(Image, structElement, controlImage);
            } else {
                if (controlImage == null)
                    output = dilatationGray(Image, structElement);
                else
                    output = geodesicDilatationGray(Image, structElement, controlImage);
            }

            return output;
        }
        private Color[,] opening(Bitmap InputImage, StructElement structElement) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
            Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

            convertImageToString(Image);
            setupProgressBar();
            if (isBinary(Image, InputImage.Size.Width, InputImage.Size.Height)) {
                output = erosionBinary(Image, structElement);
                output = dilatationBinary(output, structElement);
            }
            else {
                output = erosionGray(Image, structElement);
                output = dilatationGray(output, structElement);
            }

            return output;
        }
        private Color[,] closing(Bitmap InputImage, StructElement structElement) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
            Color[,] output = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)

            convertImageToString(Image);
            setupProgressBar();
            if (isBinary(Image, InputImage.Size.Width, InputImage.Size.Height)) {
                output = dilatationBinary(Image, structElement);
                output = erosionBinary(output, structElement);
            }
            else {
                output = dilatationGray(Image, structElement);
                output = erosionGray(output, structElement);
            }

            return output;
        }
        private Color[,] myColorInversion(Color[,] Image) {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            setupProgressBar();

            // Inversion of image
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
                    Color pixelColor = Image[x, y];                                                                     // Get the pixel color at coordinate (x,y)
                    Color updatedColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);    // Negative image
                    Image[x, y] = updatedColor;                                                                         // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                                                                          // Increment progress bar
                }
            }




            return Image;
        }
        private bool isBinary(Color[,] image, int height, int width) {
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    if (image[i, j].R != 255 && image[i, j].R != 0) {
                        return false;
                    }
                }
            }
            return true;
        }
        private Color[,] applyAND(Bitmap InputImage, Bitmap InputImage2)
        {
            if (OutputImage != null) OutputImage.Dispose();                              // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);     // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // Create array to speed-up operations (Bitmap functions are very slow)
            Color[,] Image2 = new Color[InputImage2.Size.Width, InputImage2.Size.Height];

            convertImageToString(Image);
            convertImageToString2(Image2);
            setupProgressBar();

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    if (x < InputImage2.Size.Width && y < InputImage2.Size.Width) {           //This condition is useful when you want to do an Or o
                                                                                              // a AND of Images with diffrent sizes
                        Color pixelColor = Image[x, y];                                             // Get the pixel color at coordinate (x,y)
                        Color pixelColor2 = Image2[x, y];
                        Color updatedColor;
                        if (pixelColor.R == 0 && pixelColor2.R == 0) { updatedColor = Color.FromArgb(0, 0, 0); }
                        else { updatedColor = Color.FromArgb(255, 255, 255); }
                        Image[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                        progressBar.PerformStep();                                                  // Increment progress bar
                    }
                }

            }

            return Image;

        }
        private Color[,] myApplyAND(Color[,] Image, Color[,] Image2)
        {
            if (OutputImage != null) OutputImage.Dispose();                              // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);     // Create new output image
            setupProgressBar();

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    if (x < InputImage2.Size.Width && y < InputImage2.Size.Width) {           //This condition is useful when you want to do an Or o
                                                                                              // a AND of Images with diffrent sizes
                        Color pixelColor = Image[x, y];                                             // Get the pixel color at coordinate (x,y)
                        Color pixelColor2 = Image2[x, y];
                        Color updatedColor;
                        if (pixelColor.R == 0 && pixelColor2.R == 0) { updatedColor = Color.FromArgb(0, 0, 0); }
                        else { updatedColor = Color.FromArgb(255, 255, 255); }
                        Image[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                        progressBar.PerformStep();                                                  // Increment progress bar
                    }
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
            convertImageToString2(Image2);
            setupProgressBar();

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    if (x < InputImage2.Size.Width && y < InputImage2.Size.Height) {
                        Color pixelColor = Image[x, y];                                             // Get the pixel color at coordinate (x,y)
                        Color pixelColor2 = Image2[x, y];
                        Color updatedColor;
                        if ((pixelColor.R == 255 && pixelColor2.R == 0) || (pixelColor.R == 0 && pixelColor2.R == 255)) { updatedColor = Color.FromArgb(0, 0, 0); }
                        else { updatedColor = Color.FromArgb(255, 255, 255); }
                        Image[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                        progressBar.PerformStep();                                                   // Increment progress bar

                    }
                }
            }

            return Image;

        }
        private Color[,] myApplyOR(Color[,] Image, Color[,] Image2)
        {
            if (OutputImage != null) OutputImage.Dispose();                              // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);     // Create new output image

            setupProgressBar();

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    if (x < InputImage2.Size.Width && y < InputImage2.Size.Height) {
                        Color pixelColor = Image[x, y];                                             // Get the pixel color at coordinate (x,y)
                        Color pixelColor2 = Image2[x, y];
                        Color updatedColor;
                        if ((pixelColor.R == 255 && pixelColor2.R == 0) || (pixelColor.R == 0 && pixelColor2.R == 255)) { updatedColor = Color.FromArgb(0, 0, 0); }
                        else { updatedColor = Color.FromArgb(255, 255, 255); }
                        Image[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                        progressBar.PerformStep();                                                   // Increment progress bar

                    }
                }
            }

            return Image;

        }
        private Color[,] myApplyMin(Color[,] Image, Color[,] Image2)
        {
            if (OutputImage != null) OutputImage.Dispose();                              // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);     // Create new output image

            setupProgressBar();

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    if (x < InputImage2.Size.Width && y < InputImage2.Size.Height) {
                        Color pixelColor = Image[x, y];                                             // Get the pixel color at coordinate (x,y)
                        Color pixelColor2 = Image2[x, y];
                        Color updatedColor;
                        if (pixelColor.R <= pixelColor2.R) { updatedColor = pixelColor; }
                        else { updatedColor = pixelColor2; }
                        Image[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                        progressBar.PerformStep();                                                   // Increment progress bar

                    }
                }
            }

            return Image;
        }
        private Color[,] myApplyMax(Color[,] Image, Color[,] Image2)
        {
            if (OutputImage != null) OutputImage.Dispose();                              // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);     // Create new output image

            setupProgressBar();

            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    if (x < InputImage2.Size.Width && y < InputImage2.Size.Height) {
                        Color pixelColor = Image[x, y];                                             // Get the pixel color at coordinate (x,y)
                        Color pixelColor2 = Image2[x, y];
                        Color updatedColor;
                        if (pixelColor.R >= pixelColor2.R) { updatedColor = pixelColor; }
                        else { updatedColor = pixelColor2; }
                        Image[x, y] = updatedColor;                                                 // Set the new pixel color at coordinate (x,y)
                        progressBar.PerformStep();                                                   // Increment progress bar

                    }
                }
            }

            return Image;
        }
        private HCounting valueCounting(Bitmap InputImage)
        {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height];  // Create array to speed-up operations (Bitmap functions are very slow)
            int[] histogram_r = new int[256];
            int histHeight = 128;
            Bitmap returnImage = new Bitmap(256, histHeight + 10);
            float max = 0;

            convertImageToString(Image);
            setupProgressBar();

            // Inversion of image
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Color pixelColor = Image[x, y];                                                                     // Get the pixel color at coordinate (x,y)
                    histogram_r[pixelColor.R]++;
                    if (max < histogram_r[pixelColor.R])
                        max = histogram_r[pixelColor.R];

                    progressBar.PerformStep();                                                                          // Increment progress bar
                }
            }

            using (Graphics g = Graphics.FromImage(returnImage))
            {
                for (int i = 0; i < histogram_r.Length; i++)
                {
                    float pct = histogram_r[i] / max;   // What percentage of the max is this value?
                    g.DrawLine(Pens.Black,
                        new Point(i, returnImage.Height - 5),
                        new Point(i, returnImage.Height - 5 - (int)(pct * histHeight))  // Use that percentage of the height
                        );
                }
            }

            progressBar.Visible = false;
            resultTextBox.Visible = true;
            int countedValue = histogram_r.Count(s => s != 0);
            int countBG = histogram_r[255];
            resultTextBox.Text = countedValue.ToString() + "  BG: " + countBG;

            return new HCounting(returnImage,countedValue);
        }

        private static Tuple<int, int, int> findNextContourPoint(bool[,] pixels, int x, int y, int direction)
        {
            //Using the 8-direction system
            var xDirs = new int[] { 1, 1, 0, -1, -1, -1, 0, 1 };
            var yDirs = new int[] { 0, 1, 1, 1, 0, -1, -1, -1 };


            for (int i = 0; i < 7; ++i)
            {
                int checkX = x + xDirs[direction];
                int checkY = y + yDirs[direction];

                if (pixels[checkX, checkY]) 
                    return new Tuple<int, int, int>(checkX, checkY, direction);
                else
                    direction = (direction + 1) % 8; //next direction
            }

            return new Tuple<int, int, int>(x, y, direction);
        }

        private List<Tuple<int, int>> boundaryTrace(Bitmap InputImage)
        {
            if (OutputImage != null) OutputImage.Dispose();                             // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height);    // Create new output image
            Color[,] image = new Color[InputImage.Size.Width, InputImage.Size.Height];

            convertImageToString(image);
           
            bool[,] pixels = new bool[InputImage.Size.Width + 2, InputImage.Size.Height + 2];

            List<Tuple<int,int>> result = new List<Tuple<int, int>>();

            
            for (int x = 0; x < InputImage.Size.Width + 2; x++)                                                         //Initialize matrix pixels
                for (int y = 0; y < InputImage.Height + 2; y++)
                {
                    
                    if (x == 0 || y == 0 || x == InputImage.Size.Width + 1 || y == InputImage.Height + 1)               //Extend borders
                        pixels[x, y] = false;                                                                            //background

                    else if (image[x-1,y-1].R == 255)
                        pixels[x,y] = false;                                                                       //background

                    else if (image[x-1,y-1].R == 0)
                        pixels[x,y] = true;                                                                        //foreground


                }

            //Find the first shape
            int startX = -1, startY = -1;
            for (int x = 1; x < InputImage.Size.Width + 1; ++x)
            {
                for (int y = 1; y < InputImage.Size.Height + 1; ++y)
                {
                    if (pixels[x, y] == true) //foreground pixel
                    {
                        startX = x;
                        startY = y;
                    }
                }
            }

            if (startX < 0)
                return result;

            Tuple<int,int> nextPt = findNextContourPoint(pixels, startX, startY, 0);
            int startNextX = nextPt.Item1;
            int startNextY = nextPt.Item2;
            int nextDir = nextPt.Item3;

            if (startNextX == startX && startNextY == startY)
            {
                result.Add(new Tuple<int, int>(startX - 1, startY - 1));
                return result;
            }

            result.Add(new Tuple<int, int>(startNextX - 1, startNextY - 1));

            int prevX = startX;
            int prevY = startY;
            int currX = startNextX;
            int currY = startNextY;
            int dirSearch;

            
            do{
                //Get the new search direction
                dirSearch = (nextDir + 6) % 8; 

                //Get the next boundary pixel
                nextPt = findNextContourPoint(pixels, currX, currY, dirSearch);

                prevX = currX;
                prevY = currY;
                currX = nextPt.Item1;
                currY = nextPt.Item2;
                nextDir = nextPt.Item3;


                result.Add(new Tuple<int, int>(currX - 1, currY - 1));
            }while (prevX != startX || prevY != startY || currX != startNextX || currY != startNextY);

            return result;
        }

        private string displayMembers(List<Tuple<int,int>> contours)
        {
            string result = null;
            for (int i =0 ; i< contours.Count ; i++){
                 result += "Contour " + i+ " : ";
                Tuple<int,int> point = contours.ElementAt(i);
                result += point.ToString() + ", ";
                result += ".       ";

            }

            return result;
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
        private void convertImageToString2(Color[,] Image)
        {
            // Copy input Bitmap to array            
            for (int x = 0; x < InputImage2.Size.Width; x++)
            {
                for (int y = 0; y < InputImage2.Size.Height; y++)
                {
                    Image[x, y] = InputImage2.GetPixel(x, y);                // Set pixel color in array at (x,y)
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
