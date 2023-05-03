using System;

public class NeuralNetwork
{
    public int numberOfInputs;
    public int numberOfHiddenLayers;
    public int numberOfHiddenNodesInLayer;
    public int numberOfOutputs;

    public Matrix[] layers;
    public Matrix[] biases;
    public float learningRate = 0.01f;

    public NeuralNetwork(int inputs, int hiddenLayers, int hiddenNodesPerLayer, int outputs)
    {

        if (inputs == 0 || hiddenLayers == 0 || hiddenNodesPerLayer == 0 || outputs == 0)
        {
            //throw new System.ArgumentException("Arguments can't be 0!");
        }

        this.numberOfInputs = inputs;
        this.numberOfHiddenLayers = hiddenLayers;
        this.numberOfHiddenNodesInLayer = hiddenNodesPerLayer;
        this.numberOfOutputs = outputs;

        layers = new Matrix[hiddenLayers + 1];
        biases = new Matrix[hiddenLayers + 1];

        Matrix layer;
        Matrix bias;
        for (int i = 0; i < this.numberOfHiddenLayers + 1; i++)
        {
            if (i == 0)
            {
                layer = new Matrix(hiddenNodesPerLayer, inputs);
                bias = new Matrix(hiddenNodesPerLayer, 1);
                layer = StaticMath.XavierInitialize(layer, hiddenNodesPerLayer);
            }
            else if (i == this.numberOfHiddenLayers)
            {
                layer = new Matrix(outputs, hiddenNodesPerLayer);
                bias = new Matrix(outputs, 1);
                layer = StaticMath.XavierInitialize(layer, outputs);
            }
            else
            {
                layer = new Matrix(hiddenNodesPerLayer, hiddenNodesPerLayer);
                bias = new Matrix(hiddenNodesPerLayer, 1);
                layer = StaticMath.XavierInitialize(layer, hiddenNodesPerLayer);
            }
            StaticMath.Randomize(layer);
            StaticMath.Randomize(bias);
            layers[i] = layer;
            biases[i] = bias;
        }
    }

    public float[] FeedForward(float[] inputArray)
    {
        Matrix[] nodeValues = new Matrix[numberOfHiddenLayers + 2];
        Matrix values;
        for (int i = 0; i < this.numberOfHiddenLayers + 2; i++)
        {
            if (i == 0)
            {
                values = Matrix.FromArray(inputArray);
            }
            else
            {
                values = Matrix.MatrixProduct(layers[i - 1], nodeValues[i - 1]);
                values.Add(biases[i - 1]);
                values.ApplySigmoid();
            }
            nodeValues[i] = values;
        }

        return Matrix.ToArray(nodeValues[nodeValues.Length - 1], numberOfOutputs);
    }

    public Matrix[] FeedForwardForTraining(float[] inputArray)
    {
        Matrix[] nodeValues = new Matrix[numberOfHiddenLayers + 2];
        Matrix values;
        for (int i = 0; i < this.numberOfHiddenLayers + 2; i++)
        {
            if (i == 0)
            {
                values = Matrix.FromArray(inputArray);
            }
            else
            {
                values = Matrix.MatrixProduct(layers[i - 1], nodeValues[i - 1]);
                values.Add(biases[i - 1]);
                values.ApplySigmoid();
            }
            nodeValues[i] = values;
        }

        return nodeValues;
    }

    public void TrainNetwork(float[] inputArray, float[] targetArray)
    {
        // feed forward
        Matrix[] nodeValues = FeedForwardForTraining(inputArray);

        // backpropagation
        Matrix[] errors = new Matrix[this.numberOfHiddenLayers + 1];

        // Calculate output layer error
        Matrix targets = Matrix.FromArray(targetArray);
        errors[this.numberOfHiddenLayers] = Matrix.Substract(targets, nodeValues[nodeValues.Length - 1]);

        // Calculate hidden layer errors
        for (int i = this.numberOfHiddenLayers - 1; i >= 0; i--)
        {
            Matrix transposedWeights = Matrix.Transpose(layers[i + 1]);
            errors[i] = Matrix.MatrixProduct(transposedWeights, errors[i + 1]);
        }

        // Calculate gradients
        Matrix[] gradients = new Matrix[this.numberOfHiddenLayers + 1];
        for (int i = 0; i < this.numberOfHiddenLayers + 1; i++)
        {
            gradients[i] = Matrix.ApplyDerivativeOfSigmoid(nodeValues[i + 1]);
            gradients[i].MatrixProduct(errors[i]);
            gradients[i].Multiply(this.learningRate);
        }

        // Update weights and biases
        for (int i = 0; i < this.numberOfHiddenLayers + 1; i++)
        {
            Matrix transposedInput = i > 0 ? Matrix.Transpose(nodeValues[i]) : Matrix.Transpose(nodeValues[0]);
            Matrix deltas = Matrix.MatrixProduct(gradients[i], transposedInput);

            layers[i].Add(deltas);
            biases[i].Add(gradients[i]);
        }

        // Update weights and biases
        //for (int i = 0; i < this.numberOfHiddenLayers + 1; i++)
        //{
        //    Matrix gradients = Matrix.ApplyDerivatireOfSigmoid(nodeValues[i + 1]);
        //    gradients.MatrixProduct(errors[i]);
        //    gradients.Multiply(this.learningRate);

        //    Matrix transposedInput = i > 0 ? Matrix.Transpose(nodeValues[i]) : Matrix.Transpose(nodeValues[0]);
        //    Matrix deltas = Matrix.MatrixProduct(gradients, transposedInput);

        //    layers[i].Add(deltas);
        //    biases[i].Add(gradients);
        //}
    }

    //public void TrainNetwork(float[] inputArray, float[] targetArray)
    //{
    //    // feed forward
    //    Matrix[] nodeValues = FeedForwardForTraining(inputArray);
    //    // backpropacation
    //    Matrix lastErrors = null;
    //    for (int i = 0; i < this.numberOfHiddenLayers + 1; i++)
    //    {
    //        Matrix targets;
    //        Matrix errors;
    //        if (i == 0)
    //        {
    //            targets = Matrix.FromArray(targetArray);
    //            errors = Matrix.Substract(targets, nodeValues[nodeValues.Length - (1 + i)]);
    //        }
    //        else
    //        {
    //            targets = Matrix.Transpose(layers[layers.Length - i]);
    //            errors = Matrix.MatrixProduct(targets, lastErrors);
    //        }
    //        lastErrors = errors;
    //        Matrix gradients = Matrix.ApplyDerivativeOfSigmoid(nodeValues[nodeValues.Length - (1 + i)]);
    //        gradients.MatrixProduct(errors);
    //        gradients.Multiply(this.learningRate);
    //        Matrix transposed = Matrix.Transpose(nodeValues[nodeValues.Length - (2 + i)]);
    //        Matrix deltas = Matrix.MatrixProduct(gradients, transposed);
    //        layers[layers.Length - (1 + i)].Add(deltas);
    //        biases[biases.Length - (1 + i)].Add(gradients);
    //    }
    //}

    public static NeuralNetwork Crossover(NeuralNetwork nn1, NeuralNetwork nn2, float mutationPercent)
    {
        NeuralNetwork result = new NeuralNetwork(nn1.numberOfInputs, nn1.numberOfHiddenLayers, nn1.numberOfHiddenNodesInLayer, nn1.numberOfOutputs);
        for (int i = 0; i < result.layers.Length; i++)
        {
            result.layers[i] = Matrix.Crossover(nn1.layers[i], nn2.layers[i]);
            result.layers[i] = Matrix.Mutate(result.layers[i], mutationPercent);
            result.biases[i] = Matrix.Crossover(nn1.biases[i], nn2.biases[i]);
            result.biases[i] = Matrix.Mutate(result.biases[i], mutationPercent);
        }
        return result;
    }
}