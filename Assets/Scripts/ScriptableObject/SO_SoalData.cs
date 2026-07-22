using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_SoalData", menuName = "Scriptable Objects/SO_SoalData")]
public class SO_SoalData : ScriptableObject
{
    public DifficultyLevel DifficultyLevel;
    public List<Soal> Soal = new List<Soal>();
}

[System.Serializable]
public class Soal
{
    public Sprite ImageSoal;
    [TextArea(3, 10)]
    public string TextSoal;
    public List<string> OpsiSoal = new List<string>();
    public string JawabanSoal;
}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}