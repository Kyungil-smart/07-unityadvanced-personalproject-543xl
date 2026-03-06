using System.Linq;
using UnityEngine;

public class DistrictWorldLabelUpdater : MonoBehaviour
{
    public void RefreshAllDistrictLabels()
    {
        var labels = FindObjectsOfType<DistrictWorldLabel>(true).ToList();
        foreach (var l in labels) l.Refresh();
    }
}