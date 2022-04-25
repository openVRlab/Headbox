/// <summary>
/// Calculation such as Action Unit, gaze, and more.
/// </summary>

using System.Collections.Generic;
using System.Linq;

public class Calculator
{
    /// <summary>
    /// Calculate moving average for each action unit.
    /// </summary>
    /// <param name="queue">The queue to be calculated</param>
    /// <param name="auNum">The number of Action Units</param>
    /// <returns>the list of data for each moving average of blendshape</returns>
    public float[] CalcMovingAverage(Queue<AnimationDataFrame> queue, int auNum)
    {
        float[] auValue = new float[auNum];
        // the status of queue won't be changed when using foreach statement
        foreach (var frame in queue)   
        {
            // for each index of Action Unit
            for (int i = 0; i < auNum; i++)
            {
                // store all blendshape data for `queue.Length` times 
                auValue[i] += frame.d[i];
            }
        }
        // calc and store average for each au
        float[] blendshapeMovAve = new float[auNum];
        for (int i = 0; i < auNum; i++)
        {
            blendshapeMovAve[i] = auValue[i] / queue.Count;
        }
        return blendshapeMovAve;
    }
}