#ifndef __QUATERNION__
#define __QUATERNION__

#define HALF_DEG2RAD 8.72664625e-3

float4 quaternion(float3 normalizedAxis, float degree) {
	float rad = degree * HALF_DEG2RAD;
	return float4(normalizedAxis * sin(rad), cos(rad));
}
float4 qmul(float4 a, float4 b) {
	return float4(a.w * b.xyz + b.w * a.xyz + cross(a.xyz, b.xyz), a.w * b.w - dot(a.xyz, b.xyz));
}
float3 qrotate(float4 q, float3 v) {
	return qmul(qmul(q, float4(v, 0)), float4(-q.xyz, q.w)).xyz;
}
float3 qrotateinv(float4 q, float3 v) {
	return qmul(qmul(float4(-q.xyz, q.w), float4(v, 0)), q).xyz;
}

// Rotate a vector with a rotation quaternion.
// http://mathworld.wolfram.com/Quaternion.html
float3 rotateWithQuaternion(float3 v, float4 r)
{
    float4 r_c = r * float4(-1, -1, -1, 1);
    return qmul(r, qmul(float4(v, 0), r_c)).xyz;
}

float4 getAngleAxisRotation(float3 axis, float angle){
	axis = normalize(axis);
	float s,c;
	sincos(angle,s,c);
	return float4(axis.x*s,axis.y*s,axis.z*s,c);
}

float3 rotateAngleAxis(float3 v, float3 axis, float angle){
	float4 q = getAngleAxisRotation(axis,angle);
	return rotateWithQuaternion(v,q);
}

float4 fromToRotation(float3 from, float3 to){
	float3
		v1 = normalize(from),
		v2 = normalize(to),
		cr = cross(v1,v2);
	float4 q = float4( cr,1+dot(v1,v2) );
	return normalize(q);
}

// Quaternion Lerp
float4 qlerp(float4 from, float4 to, float t) {
	float4 r;
	float t_ = 1.0 - t;
	r.x = t_ * from.x + t * to.x;
	r.y = t_ * from.y + t * to.y;
	r.z = t_ * from.z + t * to.z;
	r.w = t_ * from.w + t * to.w;
	return normalize(r);

}

// Quaternion SLerp
float4 qslerp(float4 from, float4 to, float t) {
	float cosHalfTheta = dot(from, to);
	if (abs(cosHalfTheta) >= 1.0) {
		return from;
	}

	float halfTheta = acos(cosHalfTheta);
	float sinHalfTheta = sqrt(1.0 - cosHalfTheta * cosHalfTheta);
	if (abs(sinHalfTheta) < 0.0001) {
		return from * 0.5 + to * 0.5;
	}

	float ratioA = sin((1.0 - t) * halfTheta) / sinHalfTheta;
	float ratioB = sin(t * halfTheta) / sinHalfTheta;

	return from * ratioA + to * ratioB;
}

float4 lookAt(float3 from, float3 to) {
	float3 forward = normalize(to - from);
	
	float dot_ = dot(float3(0, 0, 1), forward);
	if (abs(dot_ - (-1.0)) < 0.000001) {
		return float4(0, 1, 0, 3.1415926535897932);
	}
	if (abs(dot_ - (1.0)) < 0.000001) {
		return float4(0, 0, 0, 1);
	}

	float rotAngle = acos(dot_);
	float3 rotAxis = cross(float3(0, 0, 1), forward);
	rotAxis = normalize(rotAxis);
	return getAngleAxisRotation(rotAxis, rotAngle);
}

float4 lookRotation(float3 forward, float3 up)
{
	forward = normalize(forward);
	float3 right = normalize(cross(up, forward));

	up = cross(forward, right);
	float m00 = right.x;
	float m01 = right.y;
	float m02 = right.z;
	float m10 = up.x;
	float m11 = up.y;
	float m12 = up.z;
	float m20 = forward.x;
	float m21 = forward.y;
	float m22 = forward.z;


	float num8 = (m00 + m11) + m22;
	float4 quaternion;
	if (num8 > 0.0)
	{
		float num = sqrt(num8 + 1);
		quaternion.w = num * 0.5;
		num = 0.5 / num;
		quaternion.x = (m12 - m21) * num;
		quaternion.y = (m20 - m02) * num;
		quaternion.z = (m01 - m10) * num;
		return quaternion;
	}
	if ((m00 >= m11) && (m00 >= m22))
	{
		float num7 = sqrt(((1.0 + m00) - m11) - m22);
		float num4 = 0.5 / num7;
		quaternion.x = 0.5 * num7;
		quaternion.y = (m01 + m10) * num4;
		quaternion.z = (m02 + m20) * num4;
		quaternion.w = (m12 - m21) * num4;
		return quaternion;
	}
	if (m11 > m22)
	{
		float num6 = sqrt(((1.0 + m11) - m00) - m22);
		float num3 = 0.5 / num6;
		quaternion.x = (m10 + m01) * num3;
		quaternion.y = 0.5 * num6;
		quaternion.z = (m21 + m12) * num3;
		quaternion.w = (m20 - m02) * num3;
		return quaternion;
	}
	float num5 = sqrt(((1.0 + m22) - m00) - m11);
	float num2 = 0.5 / num5;
	quaternion.x = (m20 + m02) * num2;
	quaternion.y = (m21 + m12) * num2;
	quaternion.z = 0.5 * num5;
	quaternion.w = (m01 - m10) * num2;
	return quaternion;
}
#endif
