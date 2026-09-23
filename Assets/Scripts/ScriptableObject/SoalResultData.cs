using UnityEngine;

[CreateAssetMenu(fileName = "SoalResultData", menuName = "Scriptable Objects/SoalResultData")]
public class SoalResultData : ScriptableObject
{
    public static int TotalSoal;
    public static int WrongCount;

    public static int CorrectCount => TotalSoal - WrongCount;
}
