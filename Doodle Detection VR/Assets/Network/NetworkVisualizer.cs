using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkVisualizer : MonoBehaviour
{
    public void DrawNetwork(NeuralNetwork network, int size, int layerGap, Color neuronColor, Color connectionStrong, Color connectionWeak, Color background)
    {
        List<int> lineToX = new List<int>();
        List<int> lineToY = new List<int>();
        int startIndex = 0;
        int endIndex = 0;
        int nodesInLayer = 0;
        int neuronSize = GetNeuronSize(network, size);
        int size2 = (network.numberOfHiddenLayers + 3) * neuronSize * layerGap;
        Texture2D texture = new Texture2D(size2, size);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size2, size), Vector2.zero);
        GetComponent<SpriteRenderer>().sprite = sprite;
        SetBackground(size2, size, texture, background);

        for (int i = 0; i <= network.layers.Length; i++)
        {
            int nodeY;
            int nodeX;
            if (i == 0)
            {
                for (int j = 0; j < network.numberOfInputs; j++)
                {
                    nodeY = (size / 2) + (neuronSize * (network.numberOfInputs * 2 - 2)) - (neuronSize * j * 4);
                    nodeX = neuronSize * (i + 1) * layerGap;
                    //DrawEllipse(nodeX, nodeY, neuronSize, neuronSize, texture, neuronColor);
                    lineToX.Add(nodeX);
                    lineToY.Add(nodeY);
                    endIndex++;
                    nodesInLayer++;
                }
            } else if (i == network.layers.Length)
            {
                startIndex = endIndex - nodesInLayer;
                for (int j = 0; j < network.numberOfOutputs; j++)
                {
                    nodeY = (size / 2) + (neuronSize * (network.numberOfOutputs * 2 - 2)) - (neuronSize * j * 4);
                    nodeX = neuronSize * (i + 1) * layerGap;
                    //DrawEllipse(nodeX, nodeY, neuronSize, neuronSize, texture, neuronColor);
                    int colorIndex = 0;
                    for (int k = startIndex; k < endIndex; k++)
                    {
                        Color color = GetConnectionColor(network.layers[i - 1].data[j, colorIndex], connectionStrong, connectionWeak);
                        DrawLine(nodeX, nodeY, lineToX[k], lineToY[k], texture, color);
                        colorIndex++;
                    }
                    lineToX.Add(nodeX);
                    lineToY.Add(nodeY);
                }
            } else
            {
                startIndex = endIndex - nodesInLayer;
                nodesInLayer = 0;
                for (int j = 0; j < network.numberOfHiddenNodesInLayer; j++)
                {
                    nodeY = (size / 2) + (neuronSize * (network.numberOfHiddenNodesInLayer * 2 - 2)) - (neuronSize * j * 4);
                    nodeX = neuronSize * (i + 1) * layerGap;
                    //DrawEllipse(nodeX, nodeY, neuronSize, neuronSize, texture, neuronColor);
                    int colorIndex = 0;
                    for (int k = startIndex; k < endIndex - j; k++)
                    {
                        Color color = GetConnectionColor(network.layers[i - 1].data[j, colorIndex], connectionStrong, connectionWeak);
                        DrawLine(nodeX, nodeY, lineToX[k], lineToY[k], texture, color);
                        colorIndex++;
                    }
                    lineToX.Add(nodeX);
                    lineToY.Add(nodeY);
                    endIndex++;
                    nodesInLayer++;
                }
            }
        }
        for (int i = 0; i < lineToX.Count; i++)
        {
            DrawEllipse(lineToX[i], lineToY[i], neuronSize, neuronSize, texture, neuronColor);
        }
        texture.Apply();
    }

    private Color GetConnectionColor (float connection, Color strong, Color weak)
    {
        strong.r += connection * strong.r;
        strong.g += connection * strong.g;
        strong.b += connection * strong.b;
        weak.r -= connection * weak.r;
        weak.g -= connection * weak.g;
        weak.b -= connection * weak.b;

        return new Color(strong.r + weak.r, strong.g + weak.g, strong.b + weak.b);
    }

    private int GetNeuronSize (NeuralNetwork nn, int canvasSize)
    {
        int nCols = nn.numberOfHiddenNodesInLayer;
        if (nn.numberOfInputs >= nn.numberOfHiddenNodesInLayer && nn.numberOfInputs >= nn.numberOfOutputs)
        {
            nCols = nn.numberOfInputs;
        } else if (nn.numberOfOutputs >= nn.numberOfHiddenNodesInLayer && nn.numberOfOutputs >= nn.numberOfInputs)
        {
            nCols = nn.numberOfOutputs;
        }
        return canvasSize / (nCols * 5);
    }

    private void SetBackground(int width, int height, Texture2D texture, Color color)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }
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

    private void DrawEllipse (int startX, int startY, int width, int height, Texture2D texture, Color color)
    {
        int hh = height * height;
        int ww = width * width;
        int hhww = hh * ww;
        int x0 = width;
        int dx = 0;

        for (int x = -width; x <= width; x++)
        {
            texture.SetPixel(startX + x, startY, color);
        }

        for (int y = 1; y <= height; y++)
        {
            int x1 = x0 - (dx - 1);
            for (; x1 > 0; x1--)
                if (x1 * x1 * hh + y * y * ww <= hhww)
                    break;
            dx = x0 - x1;
            x0 = x1;

            for (int x = -x0; x <= x0; x++)
            {
                texture.SetPixel(startX + x, startY - y, color);
                texture.SetPixel(startX + x, startY + y, color);
            }
        }
    }
}
