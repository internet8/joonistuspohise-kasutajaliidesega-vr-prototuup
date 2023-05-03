using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class Drawing : MonoBehaviour
{
    public Texture2D emptyCanvas;
    public TMP_Text answer;
    public bool doorDisabled = true;
    public AudioClip spawn;
    bool drawingInProgress = false;
    bool answersActive = false;
    int answerIndex = -1;
    Texture2D canvas;
    Vector2 currentPoint, lastPoint;
    NeuralNetwork objectsNetwork;
    public GameObject[] spawnableObjects;
    string[] objectList = { "Kirve tekitamine.", "Paat", "Kera tekitamine.", "Kella vaatamine.", "Rakenduse sulgemine.", "Haamri tekitamine.", "Laterna tekitamine", "Sae tekitamine.", "Kuubiku tekitamine.", "Puu", "Tetraeedri tekitamine." };
    List<Vector3> points = new List<Vector3>();
    int upperY = 27;
    int lowerY = 0;
    int rightX = 27;
    int leftX = 0;
    float lastGuessTime = 0;
    float guessDelay = 0.33f;
    AudioSource audioSource;
    LineRenderer lineRenderer;
    Inputs inputs;
    public GameObject rightController;
    public GameObject XROrigin;
    public float spawnDistFromPlayer = 3;
    public GameObject spawnPoint;
    public GameObject line;
    bool newLineCreated = true;
    List<GameObject> lines = new List<GameObject>();

    // ads
    public GameObject ratControllerObj;
    HivemindRatController hmrc;

    // Start is called before the first frame update
    void Start()
    {
        // network
        string path = Application.dataPath + "\\Network\\network_500_shift.json";
        //TextAsset jsonNetworkFile = Resources.Load<TextAsset>("network_2x400.json");
        //Debug.Log(jsonNetworkFile.name);
        StreamReader reader = new StreamReader(path);
        string jsonStrin = reader.ReadToEnd();
        reader.Close();
        objectsNetwork = Newtonsoft.Json.JsonConvert.DeserializeObject<NeuralNetwork>(jsonStrin);
        //Debug.Log(objectsNetwork.layers[0].data[0, 0]);
        inputs = XROrigin.GetComponent<Inputs>();
        audioSource = GetComponent<AudioSource>();

        ResetCanvas();

        // ads
        hmrc = ratControllerObj.GetComponent<HivemindRatController>();
    }

    // Update is called once per frame
    void Update()
    {
        int layerMask = 1 << 7;
        RaycastHit hit;
        Ray ray = new Ray(rightController.transform.position, rightController.transform.forward);
        Debug.DrawRay(rightController.transform.position, rightController.transform.forward, Color.green);
        if (inputs.drawing)
        {
            if (Physics.Raycast(ray, out hit, 100, layerMask))
            {
                lastPoint = currentPoint;
                currentPoint = new Vector2(canvas.width * hit.textureCoord.x, canvas.height * hit.textureCoord.y);
                if (Mathf.Abs(lastPoint.x - currentPoint.x) <= 1 || lastPoint.x == 0)
                {
                    canvas.SetPixel((int)(currentPoint.x), (int)(currentPoint.y), Color.black);
                    upperY = (int)currentPoint.y < upperY ? (int)currentPoint.y : upperY;
                    lowerY = (int)currentPoint.y > lowerY ? (int)currentPoint.y : lowerY;
                    leftX = (int)currentPoint.x < leftX ? (int)currentPoint.x : leftX;
                    rightX = (int)currentPoint.x > rightX ? (int)currentPoint.x : rightX;
                }
                else
                {
                    DrawLine((int)lastPoint.x, (int)lastPoint.y, (int)currentPoint.x, (int)currentPoint.y, canvas, Color.black);
                }
                points.Add(hit.point);
                lineRenderer.positionCount = points.Count;
                lineRenderer.SetPositions(points.ToArray());
                //canvas.SetPixel((int)(canvas.width * hit.textureCoord.x), (int)(canvas.height * hit.textureCoord.y), Color.white);
                canvas.Apply();
                drawingInProgress = true;
                newLineCreated = false;
            }
            if (drawingInProgress)
            {
                if (!answersActive)
                {
                    answersActive = true;
                }
                if (Time.time > lastGuessTime + guessDelay)
                {
                    lastGuessTime = Time.time;
                    float[] input = PixelsToInputs(canvas.GetPixels());
                    input = CenterImageX(leftX, rightX, input);
                    input = CenterImageY(upperY, lowerY, input);
                    Guess(input);
                }
            }
        }
        else if (inputs.inserting && drawingInProgress)
        {
            if (answerIndex != 3) audioSource.PlayOneShot(spawn, 1.5f);
            if (answerIndex == 3)
            {
                spawnableObjects[3].GetComponent<ClockAnimation>().Animate();
            } else if (answerIndex == 4)
            {
                spawnableObjects[4].GetComponent<Door>().SpawnDoor();
            }
            else
            {
                GameObject clone = Instantiate(spawnableObjects[answerIndex]);
                //clone.transform.position = spawnPoint.transform.position;
                Vector3 forwardTemp = new Vector3(Camera.main.transform.forward.x, 2, Camera.main.transform.forward.z);
                clone.transform.position = XROrigin.transform.position + forwardTemp * spawnDistFromPlayer;
                if (answerIndex == 0)
                {
                    hmrc.AddBait(clone);
                }
                else if (answerIndex == 5)
                {
                    hmrc.AddBait(clone);
                }
                else if (answerIndex == 6)
                {
                    hmrc.AddLight(clone);
                }
                else if (answerIndex == 7)
                {
                    hmrc.AddBait(clone);
                }
            }
            ResetCanvas();
        }
        else if (!inputs.drawing)
        {
            if (!newLineCreated)
            {
                newLineCreated = true;
                GameObject newLine = Instantiate(line);
                lines.Add(newLine);
                lineRenderer = newLine.GetComponent<LineRenderer>();
                lineRenderer.positionCount = 0;
                points = new List<Vector3>();
                lineRenderer.SetPositions(new Vector3[0]);
            }
            currentPoint = new Vector2(0, 0);
            float[] input = PixelsToInputs(canvas.GetPixels());
            input = CenterImageX(leftX, rightX, input);
            input = CenterImageY(upperY, lowerY, input);
            //ConsolePrintDoodle(input);
        }
        if (inputs.clearing)
        {
            ResetCanvas();
        }
    }

    void Guess(float[] inputs)
    {
        float[] outputs = objectsNetwork.FeedForward(inputs);
        // disable 2 quesses
        outputs[1] -= outputs[1];
        if (doorDisabled)
        {
            outputs[4] -= outputs[4];
        }
        outputs[9] -= outputs[9];
        float firstVal = -1;
        float secondVal = -1;
        float thirdVal = -1;
        int firstI = 0;
        int secondI = 0;
        int thirdI = 0;
        float sum = outputs.Sum();
        for (int i = 0; i < outputs.Length; i++)
        {
            if (firstVal <= outputs[i])
            {
                thirdVal = secondVal;
                thirdI = secondI;
                secondVal = firstVal;
                secondI = firstI;
                firstVal = outputs[i];
                firstI = i;
            }
            else if (secondVal <= outputs[i])
            {
                thirdVal = secondVal;
                thirdI = secondI;
                secondVal = outputs[i];
                secondI = i;
            }
            else if (thirdVal <= outputs[i])
            {
                thirdVal = outputs[i];
                thirdI = i;
            }
        }
        answerIndex = firstI;
        answer.gameObject.SetActive(true);
        answer.text = objectList[firstI] + "\n" + Mathf.Round(firstVal * 100 / sum).ToString() + "%";
        //Debug.Log(objectList[firstI]);
    }

    private void DrawLine(int x1, int y1, int x2, int y2, Texture2D texture, Color color)
    {
        int w = x2 - x1;
        int h = y2 - y1;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);
        if (!(longest > shortest))
        {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            texture.SetPixel(x1, y1, color);
            upperY = y1 < upperY ? y1 : upperY;
            lowerY = y1 > lowerY ? y1 : lowerY;
            leftX = x1 < leftX ? x1 : leftX;
            rightX = x1 > rightX ? x1 : rightX;
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x1 += dx1;
                y1 += dy1;
            }
            else
            {
                x1 += dx2;
                y1 += dy2;
            }
        }
    }

    float[] CenterImageY(int upperY, int lowerY, float[] input)
    {
        float[] result = new float[784];
        int freeSpace = (upperY + (27 - lowerY)) / 2;
        int freeSpaceRounded = (int)Mathf.Ceil((float)(upperY + (27 - lowerY)) / 2);
        for (int i = 0; i < result.Length; i++)
        {
            if (i < freeSpace * 28 || i >= 783 - 28 * freeSpaceRounded)
            {
                result[i] = 0f;
            }
            else
            {
                int newI = i - freeSpace * 28;
                //Debug.Log(freeSpaceRounded);
                result[i] = input[upperY * 28 + newI];
            }
        }
        return result;
    }

    float[] CenterImageX(int leftX, int rightX, float[] input)
    {
        float[] result = new float[784];
        int freeLeft = leftX;
        int freeRight = 27 - rightX;
        int moveDistDir = (freeLeft - freeRight) / 2;
        for (int i = 0; i < result.Length; i++)
        {
            int rowIndex = i % 28;
            int index = rowIndex - moveDistDir;
            result[i] = index >= 0 && index <= 27 ? input[(i / 28) * 28 + index] : 0;
        }
        return result;
    }

    float[] PixelsToInputs(Color[] pixels)
    {
        float[] result = new float[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            int rowNum = i / 28;
            int rowIndex = 27 - i % 28;
            result[28 * rowNum + rowIndex] = 1 - pixels[i].r;
        }
        return result;
    }

    public void ResetCanvas()
    {
        answersActive = false;
        drawingInProgress = false;
        answer.gameObject.SetActive(false);
        answerIndex = -1;
        currentPoint = new Vector2(2, 2);
        canvas = new Texture2D(28, 28);
        upperY = 27;
        lowerY = 0;
        leftX = 27;
        rightX = 0;
        canvas.SetPixels(emptyCanvas.GetPixels());
        canvas.filterMode = FilterMode.Point;
        canvas.Apply();
        GetComponent<Renderer>().material.mainTexture = canvas;
        for (int i = 0; i < lines.Count; i++)
        {
            Destroy(lines[i]);
        }
        lineRenderer = line.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        points = new List<Vector3>();
        lineRenderer.SetPositions(new Vector3[0]);
    }
}
