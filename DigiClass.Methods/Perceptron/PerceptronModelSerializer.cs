using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace DigiClass.Methods.Perceptron
{
    public class PerceptronModelSerializer : IModelSerializer<PerceptronModel>
    {
        public void Serialize(TextWriter writer, PerceptronModel model)
        {
            var layerSizes = string.Join(";", model.LayerSizes.Select(i => i.ToString()).ToArray());

            var biases = model.Biases.Select(m => new XElement(
                "Bias", Serialize(m)
            ));

            var weights = model.Weights.Select(m => new XElement(
                "Weight", Serialize(m)
            ));

            var doc = new XDocument(
                new XElement("Network",
                    new XElement("LayerSizes", layerSizes),
                    new XElement("Biases", biases),
                    new XElement("Weights", weights)
                ));

            writer.Write(doc.ToString());
        }

        public PerceptronModel Deserialize(TextReader xml)
        {
            var doc = XDocument.Load(xml);
            var sizes = doc.Root.Descendants("LayerSizes").First().Value.Split(';').Select(int.Parse).ToArray();

            var biases = doc.Root.Descendants("Biases").Descendants("Bias").Select(b =>
                Matrix<double>.Build.DenseOfColumnArrays(
                    b.Descendants("Column").Select(col =>
                        col.Value.Split(';').Select(d => double.Parse(d, CultureInfo.InvariantCulture)).ToArray())
                )).ToList();

            var weights = doc.Root.Descendants("Weights").Descendants("Weight").Select(b =>
                Matrix<double>.Build.DenseOfColumnArrays(
                    b.Descendants("Column").Select(col =>
                        col.Value.Split(';').Select(d => double.Parse(d, CultureInfo.InvariantCulture)).ToArray())
                )).ToList();

            return new PerceptronModel(sizes, biases, weights);
        }

        private static IEnumerable<XElement> Serialize(Matrix<double> matrix)
        {
            return matrix.ToColumnArrays().Select(column =>
            {
                return new XElement("Column",
                    string.Join(";",
                        column.Select(d => d.ToString(CultureInfo.InvariantCulture))
                    ));
            });
        }
    }
}