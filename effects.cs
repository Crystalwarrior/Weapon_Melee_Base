//Sounds
datablock AudioProfile(MeleeClangSound)
{
	filename	= "./clang.wav";
	description	= AudioClosest3d;
	preload		= true;
};

datablock AudioProfile(MeleeChargeSound)
{
	filename = "base/data/sound/clickPlant.wav";
	description = AudioClosest3d;
	preload = false;
};

datablock AudioProfile(MeleeStabSound1)
{
	filename = "./stab1.wav";
	description = AudioClosest3d;
	preload = false;
};
datablock AudioProfile(MeleeStabSound2)
{
	filename = "./stab2.wav";
	description = AudioClosest3d;
	preload = false;
};
datablock AudioProfile(MeleeStabSound3)
{
	filename = "./stab3.wav";
	description = AudioClosest3d;
	preload = false;
};
datablock AudioProfile(MeleeStabSound4)
{
	filename = "./stab4.wav";
	description = AudioClosest3d;
	preload = false;
};
datablock AudioProfile(MeleeStabSound5)
{
	filename = "./stab5.wav";
	description = AudioClosest3d;
	preload = false;
};
datablock AudioProfile(MeleeStabSwordSound)
{
	filename = "./stab_sword.wav";
	description = AudioClosest3d;
	preload = false;
};
//Emitters and juicy stuff
datablock ParticleData(MeleeSharpExplosionParticle)
{
	dragCoefficient      = 2;
	gravityCoefficient   = 1.0;
	inheritedVelFactor   = 0.4;
	constantAcceleration = 0.0;
	spinRandomMin = -90;
	spinRandomMax = 90;
	lifetimeMS           = 500;
	lifetimeVarianceMS   = 300;
	textureName          = "base/data/particles/star1";
	colors[0]     = "0.7 0.7 0.9 0.9";
	colors[1]     = "0.9 0.9 0.9 0.0";
	sizes[0]      = 0.25;
	sizes[1]      = 0.1;
};

datablock ParticleEmitterData(MeleeSharpExplosionEmitter)
{
	ejectionPeriodMS = 7;
	periodVarianceMS = 0;
	ejectionVelocity = 8;
	velocityVariance = 1.0;
	ejectionOffset   = 0.0;
	thetaMin         = 0;
	thetaMax         = 60;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;
	particles = "MeleeSharpExplosionParticle";

	uiName = "";
};

datablock ExplosionData(MeleeSharpExplosion)
{
	//explosionShape = "";
	lifeTimeMS = 500;

	soundProfile = SwordHitSound;

	particleEmitter = MeleeSharpExplosionEmitter;
	particleDensity = 10;
	particleRadius = 0.2;

	faceViewer     = true;
	explosionScale = "1 1 1";

	shakeCamera = true;
	camShakeFreq = "20.0 22.0 20.0";
	camShakeAmp = "1.0 1.0 1.0";
	camShakeDuration = 0.2;
	camShakeRadius = 5.0;

	// Dynamic light
	lightStartRadius = 3;
	lightEndRadius = 0;
	lightStartColor = "00.0 0.2 0.6";
	lightEndColor = "0 0 0";
};

datablock ProjectileData(MeleeSharpProjectile)
{
	explosion = MeleeSharpExplosion;
};

datablock ParticleData(MeleeBloodExplosionParticle)
{
	dragCoefficient      = 0;
	gravityCoefficient   = 5.0;
	inheritedVelFactor   = 0.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 100;
	lifetimeVarianceMS   = 60;
	textureName          = "base/data/particles/chunk";
	spinSpeed		= 10.0;
	spinRandomMin		= -500.0;
	spinRandomMax		= 500.0;
	colors[0]     = "0.71 0.01 0.01 0.9";
	colors[1]     = "0.71 0.01 0.01 0.6";
	sizes[0]      = 0.1;
	sizes[1]      = 0.1;
	useInvAlpha = true;
};
datablock ParticleEmitterData(MeleeBloodExplosionEmitter)
{
   ejectionPeriodMS = 15;
   periodVarianceMS = 2;
   ejectionVelocity = 4;
   velocityVariance = 2;
   ejectionOffset   = 0.0;
   thetaMin         = 0;
   thetaMax         = 80;
   phiReferenceVel  = 0;
   phiVariance      = 360;
   overrideAdvance = false;
   particles = "MeleeBloodExplosionParticle";
};

datablock ExplosionData(MeleeBloodExplosion)
{
	// soundProfile = bullethitSound;
	lifeTimeMS = 150;
	particleEmitter = MeleeBloodExplosionEmitter;
	particleDensity = 5;
	particleRadius = 0.2;
	faceViewer     = true;
	explosionScale = "1 1 1";
	shakeCamera = false;
	camShakeFreq = "0.0 1.0 1.0";
	camShakeAmp = "0.0 3.0 2.5";
	camShakeDuration = 0.5;
	camShakeRadius = 0.5;
	lightStartRadius = 0;
	lightEndRadius = 0;
};

datablock ProjectileData(MeleeBloodProjectile)
{
	explosion = MeleeBloodExplosion;
};

//Shamelessly stolen particles from dueling pack
datablock ParticleData(BluntMetalExplosionSparkParticle)
{
	dragCoefficient      = 0.5;
	gravityCoefficient   = 1.0;
	inheritedVelFactor   = 0.5;
	constantAcceleration = 0.0;
	lifetimeMS           = 400;
	lifetimeVarianceMS   = 250;
	textureName          = "base/data/particles/star1";

	spinSpeed		= 50.0;
	spinRandomMin		= -500.0;
	spinRandomMax		= 500.0;

	colors[0]     = "1.0 1.0 0.0 1.0";
	colors[1]     = "1.0 1.0 0.0 0.0";
	sizes[0]      = 0.15;
	sizes[1]      = 0.15;

	useInvAlpha = false;
};

datablock ParticleEmitterData(BluntMetalExplosionSparkEmitter)
{
	lifeTimeMS = 50;

	ejectionPeriodMS = 3;
	periodVarianceMS = 0;
	ejectionVelocity = 6;
	velocityVariance = 1.0;
	ejectionOffset   = 0.0;
	thetaMin         = 0;
	thetaMax         = 95;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;
	particles = BluntMetalExplosionSparkParticle;

	uiName = "Blunt Metal Dust";
	emitterNode = HalfEmitterNode;
};


datablock ExplosionData(BladeSmallMetalExplosion)
{
	//explosionShape = "";
	soundProfile = MeleeClangSound;

	lifeTimeMS = 150;

	particleEmitter = BluntMetalExplosionSparkEmitter;
	particleDensity = 4;
	particleRadius = 0.25;

	faceViewer     = true;
	explosionScale = "1 1 1";

	shakeCamera = true;
	camShakeFreq = "20.0 22.0 20.0";
	camShakeAmp = "1.0 1.0 1.0";
	camShakeDuration = 0.35;
	camShakeRadius = 8.5;

	// Dynamic light
	lightStartRadius = 3;
	lightEndRadius = 0.0;
	lightStartColor = "0.4 0.4 0.0 0.6";
	lightEndColor = "0 0 0";
};

datablock ProjectileData(MeleeBlockProjectile)
{
	explosion = BladeSmallMetalExplosion;
};