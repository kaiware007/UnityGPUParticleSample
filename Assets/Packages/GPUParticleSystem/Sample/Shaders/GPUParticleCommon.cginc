#ifndef GPUPARTICLE_COMMON_INCLUDED
#define GPUPARTICLE_COMMON_INCLUDED

StructuredBuffer<uint> _ParticleActiveList;
StructuredBuffer<uint> _InViewsList;

uint GetParticleIndex(int index) {
#ifdef GPUPARTICLE_CULLING_ON
	return _InViewsList[index];
#else
	return _ParticleActiveList[index];
#endif
}

#endif // GPUPARTICLE_COMMON_INCLUDED
