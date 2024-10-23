using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.IO;

public class FaceRecognitionService
{
    private readonly CascadeClassifier _faceDetector;

    public FaceRecognitionService()
    {
        string classifierPath = @"D:\Finabits Employee ManagementApp\FinabitEmployee\FaceRecognitionResources\Resources\haarcascade_frontalface_default.xml";

        _faceDetector = new CascadeClassifier(classifierPath);
    }

    public bool CompareFaces(string imagePath1, string imagePath2)
    {
        var img1 = new Image<Bgr, byte>(imagePath1);
        var img2 = new Image<Bgr, byte>(imagePath2);

        var faces1 = _faceDetector.DetectMultiScale(img1, 1.1, 10, Size.Empty);
        var faces2 = _faceDetector.DetectMultiScale(img2, 1.1, 10, Size.Empty);

        // Simple face matching based on detected face features
        if (faces1.Length > 0 && faces2.Length > 0)
        {
            // Compare faces in a simplified way (you can apply more advanced techniques here)
            return true; // Assuming for now it matches
        }

        return false;
    }
}
    