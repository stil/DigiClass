using System;
using System.Collections.Generic;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;

namespace DigiClass.Methods
{
    /// <summary>
    ///     Represents classifier algorithm.
    /// </summary>
    public interface IClassifier
    {
        /// <summary>
        ///     Fired when training iteration has just completed.
        ///     First argument is a number of previous iteration, starts with 1.
        /// </summary>
        event Action<int> IterationComplete;

        /// <summary>
        ///     Feeds classifier with training data.
        /// </summary>
        /// <param name="trainingData">Training data.</param>
        /// <param name="cancellationToken">Training cancellation token.</param>
        void Train(
            IEnumerable<IDataPoint> trainingData,
            CancellationToken cancellationToken);

        /// <summary>
        ///     Classifies given input with trained model.
        /// </summary>
        /// <param name="input">Input vector.</param>
        Vector<double> Classify(Vector<double> input);
    }
}