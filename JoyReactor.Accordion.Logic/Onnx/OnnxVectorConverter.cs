using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace JoyReactor.Accordion.Logic.Onnx;

public class OnnxVectorConverter(
    InferenceSession inferenceSession,
    IOptions<OnnxSettings> settings)
    : IOnnxVectorConverter
{
    protected static readonly float[] Mean = [0.48145466f, 0.4578275f, 0.40821073f];
    protected static readonly float[] Std = [0.26862954f, 0.26130258f, 0.27577711f];

    public async Task<float[]> ConvertAsync(Image<Rgb24> image)
    {
        var inputSize = settings.Value.InputSize;
        var outputSize = settings.Value.OutputSize;

        var input = ConvertToTensor(image, inputSize);
        var output = new float[outputSize];

        using var inputValue = OrtValue.CreateTensorValueFromMemory(
            OrtMemoryInfo.DefaultInstance,
            input.Buffer,
            [1, 3, inputSize, inputSize]);

        using var outputValue = OrtValue.CreateTensorValueFromMemory(
            OrtMemoryInfo.DefaultInstance,
            output.AsMemory(),
            [1, outputSize]);

        await inferenceSession.RunAsync(
            new RunOptions(),
            [settings.Value.InputName],
            [inputValue],
            [settings.Value.OutputName],
            [outputValue]);

        L2Normalize(output);

        return output;
    }

    protected static DenseTensor<float> ConvertToTensor(Image<Rgb24> image, int size)
    {
        var tensor = new DenseTensor<float>([1, 3, size, size]);
        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < size; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (var x = 0; x < size; x++)
                {
                    tensor[0, 0, y, x] = (row[x].R / 255f - Mean[0]) / Std[0];
                    tensor[0, 1, y, x] = (row[x].G / 255f - Mean[1]) / Std[1];
                    tensor[0, 2, y, x] = (row[x].B / 255f - Mean[2]) / Std[2];
                }
            }
        });

        return tensor;
    }

    protected static void L2Normalize(float[] vector)
    {
        var sum = 0f;
        for (var i = 0; i < vector.Length; i++)
            sum += vector[i] * vector[i];

        var norm = MathF.Sqrt(sum);
        if (norm < 1e-10f)
            return;

        var invNorm = 1.0f / norm;
        for (var i = 0; i < vector.Length; i++)
            vector[i] *= invNorm;
    }
}

public interface IOnnxVectorConverter
{
    Task<float[]> ConvertAsync(Image<Rgb24> image);
}