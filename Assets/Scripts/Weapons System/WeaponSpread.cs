using UnityEngine;

public class WeaponSpread
{
    private AnimationCurve spreadCurve;
    private float baseSpread;
    private float maxSpread;
    private float fireRateRPM;

    public float CurrentSpreadPosition { get; private set; } = 0f;
    public float CurrentSpreadAmount { get; private set; } = 0f;

    private float lastShotTime = 0f;

    public WeaponSpread(float baseSpread, float halfSpread, float maxSpread, float magSize, float fireRateRPM)
    {
        this.baseSpread = baseSpread;
        this.maxSpread = maxSpread;
        this.fireRateRPM = fireRateRPM;

        CreateSpreadCurve(baseSpread, halfSpread, maxSpread);
    }

    private void CreateSpreadCurve(float baseSpread, float halfSpread, float maxSpread)
    {
        spreadCurve = new AnimationCurve();

        // key 0: BaseSpread at start
        spreadCurve.AddKey(baseSpread, baseSpread);

        // key 1: HalfSpread at middle
        spreadCurve.AddKey(halfSpread, halfSpread);

        // key 2: MaxSpread at end
        spreadCurve.AddKey(maxSpread, maxSpread);
    }

    public void OnShotFired(float magSize)
    {
        // increase spread based on shot
        CurrentSpreadPosition += 1f / magSize; // x axis is shots fired
        CurrentSpreadPosition = Mathf.Clamp01(CurrentSpreadPosition);

        lastShotTime = Time.time;

        CalculateSpread();
    }

    public void UpdateSpreadOverTime()
    {
        float timeSinceLastShot = Time.time - lastShotTime;
        float maxFireInterval = 60f / fireRateRPM;

        if (CurrentSpreadPosition > baseSpread)
        {
            // scale recovery based on how much slower than max fire rate
            float recoveryScale = Mathf.Clamp01((timeSinceLastShot - maxFireInterval) / maxFireInterval);

            float recoverySpeed = (maxSpread - baseSpread) * recoveryScale;

            CurrentSpreadPosition -= recoverySpeed * Time.deltaTime;

            if (CurrentSpreadPosition < baseSpread)
            {
                CurrentSpreadPosition = baseSpread;
            }

            CalculateSpread();
        }
    }

    public void ResetSpread()
    {
        CurrentSpreadPosition = baseSpread;
        CalculateSpread();
    }

    private void CalculateSpread()
    {
        CurrentSpreadAmount = spreadCurve.Evaluate(CurrentSpreadPosition);
    }
}
