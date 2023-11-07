# Image Processing Application

This project is an Image Processing Application implemented in C#. It's aim is to build a set of functionalities (e.g. filter application, Canny edge detection, Hough transform) to design and implement a pipeline for wall socket detection. Images can be loaded within the graphical user interface (GUI) and exported. This project was developed as part of the Image Processing course at the University of Utrecht during an Erasmus exchange program.

For detailed info about the pipeline and results analysis, feel free to look at the [final report](https://github.com/MarognaLorenzo/ImageProcessing/blob/master/final_report.pdf)

## Features

- **Function Application:** Users can apply various image processing functions to test some partial results in the pipeline.
- **Socket detection:** The pipeline was tested on real world images and proved to be able to find front-viewed sockets in a low-noise environ.
- **Socket recognition:** The programm assigns a different color to the bounding box accordingly to the kind socket. It can distinguish between German, French, British and Itaian.
- **User-Friendly GUI:** The graphical user interface (GUI) is designed for ease of use, with intuitive controls for loading, editing, and exporting images.
- **Export Images:** Users can save the edited images in various file formats, such as JPEG, PNG, or BMP.

## Getting Started

Follow these steps to get the project up and running on your local machine:

1. Clone the repository:
```bash
  https://github.com/MarognaLorenzo/ImageProcessing.git
```

2. Open the solution in Visual Studio or your preferred C# development environment.
3. Build and run the Application

## Usage

- Launch the application and use the GUI to load an image from your local storage.
- Test the pipeline out
- Enjoy ( sometimes ) the result 
- Export the edited image in your desired file format.
    
## Contributing

Contributions are welcome! If you'd like to contribute to the project, please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bug fix:
3. Make your changes and commit them
4. Push your changes to your fork
5. Create a pull request on the original repository, explaining your changes and why they should be merged
