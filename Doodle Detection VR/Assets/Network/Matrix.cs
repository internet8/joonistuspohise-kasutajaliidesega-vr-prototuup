using System;

// very simple naive matrix math, nothing fancy
public class Matrix
{
    public int rows;
    public int columns;
    public float[,] data;

    public Matrix(int rows, int cols)
    {
        this.rows = rows;
        this.columns = cols;
        this.data = new float[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                this.data[i, j] = 0f;
            }
        }
    }

    public static Matrix MatrixProduct(Matrix m1, Matrix m2)
    {
        Matrix result = new Matrix(m1.rows, m2.columns);
        for (int i = 0; i < result.rows; i++)
        {
            for (int j = 0; j < result.columns; j++)
            {
                float sum = 0;
                for (int k = 0; k < m1.columns; k++)
                {
                    sum += m1.data[i, k] * m2.data[k, j];
                }
                result.data[i, j] = sum;
            }
        }
        return result;
    }

    public void MatrixProduct(Matrix m)
    {
        for (int i = 0; i < this.rows; i++)
        {
            for (int j = 0; j < this.columns; j++)
            {
                this.data[i, j] *= m.data[i, j];
            }
        }
    }

    public static Matrix Transpose(Matrix m)
    {
        Matrix result = new Matrix(m.columns, m.rows);
        for (int i = 0; i < m.rows; i++)
        {
            for (int j = 0; j < m.columns; j++)
            {
                result.data[j, i] = m.data[i, j];
            }
        }
        return result;
    }

    public void Multiply(float n)
    {
        for (int i = 0; i < this.rows; i++)
        {
            for (int j = 0; j < this.columns; j++)
            {
                this.data[i, j] *= n;
            }
        }
    }

    public void ApplySigmoid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                data[i, j] = (float)(1 / (1 + Math.Pow(Math.E, -data[i, j])));
            }
        }
    }

    public static Matrix ApplyDerivativeOfSigmoid(Matrix m)
    {
        Matrix result = new Matrix(m.rows, m.columns);
        for (int i = 0; i < m.rows; i++)
        {
            for (int j = 0; j < m.columns; j++)
            {
                result.data[i, j] = (float)(m.data[i, j] * (1 - m.data[i, j]));
            }
        }
        return result;
    }

    public void ApplyReLU()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                data[i, j] = Math.Max(0, data[i, j]);
            }
        }
    }

    public static Matrix ApplyDerivativeOfReLU(Matrix m)
    {
        Matrix result = new Matrix(m.rows, m.columns);
        for (int i = 0; i < m.rows; i++)
        {
            for (int j = 0; j < m.columns; j++)
            {
                result.data[i, j] = m.data[i, j] > 0 ? 1 : 0;
            }
        }
        return result;
    }

    public void Add(Matrix m)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                data[i, j] += m.data[i, j];
            }
        }
    }

    public static Matrix Substract(Matrix m1, Matrix m2)
    {
        Matrix result = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < result.rows; i++)
        {
            for (int j = 0; j < result.columns; j++)
            {
                result.data[i, j] = m1.data[i, j] - m2.data[i, j];
            }
        }
        return result;
    }

    public static Matrix BinaryCrossEntropyError(Matrix targets, Matrix predictions)
    {
        Matrix errors = new Matrix(targets.rows, targets.columns);
        for (int i = 0; i < errors.rows; i++)
        {
            for (int j = 0; j < errors.columns; j++)
            {
                float target = targets.data[i, j];
                float prediction = predictions.data[i, j];
                errors.data[i, j] = -target * (float)Math.Log(prediction) - (1 - target) * (float)Math.Log(1 - prediction);
            }
        }
        return errors;
    }

    public static Matrix FromArray(float[] array)
    {
        Matrix m = new Matrix(array.Length, 1);
        for (int i = 0; i < array.Length; i++)
        {
            m.data[i, 0] = array[i];
        }
        return m;
    }

    public static float[] ToArray(Matrix m, int size)
    {
        float[] result = new float[size];
        for (int i = 0; i < size; i++)
        {
            result[i] = m.data[i, 0];
        }
        return result;
    }

    public static Matrix Crossover(Matrix m1, Matrix m2)
    {
        Matrix result = new Matrix(m1.rows, m1.columns);
        for (int i = 0; i < m1.rows; i++)
        {
            for (int j = 0; j < m1.columns; j++)
            {
                if (StaticMath.GetRandomInteger(0, 2) == 1)
                {
                    result.data[i, j] = m1.data[i, j];
                }
                else
                {
                    result.data[i, j] = m2.data[i, j];
                }
            }
        }
        return result;
    }

    public static Matrix Mutate(Matrix m, float percent)
    {
        Matrix result = new Matrix(m.rows, m.columns);
        for (int i = 0; i < m.rows; i++)
        {
            for (int j = 0; j < m.columns; j++)
            {
                if (StaticMath.GetRandomFloat(0, 100) < percent)
                {
                    result.data[i, j] = StaticMath.GetRandomFloat(-1.0f, 1.0f);
                }
                else
                {
                    result.data[i, j] = m.data[i, j];
                }
            }
        }
        return result;
    }
}