#ifndef COLORUTIL_INCLUDED
#define COLORUTIL_INCLUDED

/// hue = 0~360
fixed3 ShiftColorHSV(fixed3 rgb, fixed hue, fixed sat, fixed val)
{
	fixed3 result = rgb;
	float VSU = val * sat * cos(hue * 3.14159265 / 180);
	float VSW = val * sat * sin(hue * 3.14159265 / 180);

	result.x = (0.299 * val + 0.701 * VSU + 0.168 * VSW) * rgb.x
		+ (0.587 * val - 0.587 * VSU + 0.330 * VSW) * rgb.y
		+ (0.114 * val - 0.114 * VSU - 0.497 * VSW) * rgb.z;

	result.y = (0.299 * val - 0.299 * VSU - 0.328 * VSW) * rgb.x
		+ (0.587 * val + 0.413 * VSU + 0.035 * VSW) * rgb.y
		+ (0.114 * val - 0.114 * VSU + 0.292 * VSW) * rgb.z;

	result.z = (0.299 * val - 0.3 * VSU + 1.25 * VSW) * rgb.x
		+ (0.587 * val - 0.588 * VSU - 1.05 * VSW) * rgb.y
		+ (0.114 * val + 0.886 * VSU - 0.203 * VSW) * rgb.z;

	return result;
}

fixed3 Hue(fixed hue) {
	fixed3 rgb = frac(hue + fixed3(0.0, 2.0 / 3.0, 1.0 / 3.0));

	rgb = abs(rgb * 2.0 - 1.0);
	return clamp(rgb * 3.0 - 1.0, 0.0, 1.0);
}

/// hue = 0~360
fixed3 GetColorHSV(fixed hue, fixed sat, fixed val)
{
	return ((Hue(hue / 360.0) - 1.0) * sat + 1.0) * val;
}
#endif // COLORUTIL_INCLUDED