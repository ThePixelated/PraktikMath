using UnityEngine;
using System;

public class ClockController : MonoBehaviour
{
    public Transform hourHand;
    public Transform minuteHand;
    public Transform secondHand;

    public void SetClock(int hour, int minute, int second)
    {
        float secondRotation = second * 6f;
        secondHand.localRotation = Quaternion.Euler(0, 0, -secondRotation);

        float minuteRotation = (minute * 6f) + (second * 0.1f);
        minuteHand.localRotation = Quaternion.Euler(0, 0, -minuteRotation);

        float hourRotation = ((hour % 12) * 30f) + (minute * 0.5f);
        hourHand.localRotation = Quaternion.Euler(0, 0, -hourRotation);
    }

    public void SetClockFromstring(string timeString)
    {
        // Memecah string "14:15" berdasarkan karakter ":"
        string[] timeParts = timeString.Split(':');

        if (timeParts.Length >= 2)
        {
            int hour = int.Parse(timeParts[0]);
            int minute = int.Parse(timeParts[1]);
            int second = 0; // Karena di soal cuma ada jam dan menit

            //string[] timeParts = timeString.Split(':');
            //int hour = int.Parse(timeParts[0]);
            //int minute = int.Parse(timeParts[1]);
            //int second = timeParts.Length > 2 ? int.Parse(timeParts[2]) : 0;

            // Panggil fungsi rotasi yang sudah kita buat tadi
            SetClock(hour, minute, second);
        }
    }
}