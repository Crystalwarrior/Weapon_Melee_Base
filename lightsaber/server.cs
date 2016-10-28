//lightSaber.cs
datablock AudioProfile(lightSaberDrawSound)
{
	filename    = "./2_LightsaberOn.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(lightSaberHitSound)
{
	filename    = "./2_LightSaberHit.wav";
	description = AudioClosest3d;
	preload = true;
};

datablock AudioProfile(lightSaberSwingSound1)
{
	filename    = "./2_LightsaberSwing1.wav";
	description = AudioClosest3d;
	preload = true;
};

datablock AudioProfile(lightSaberSwingSound2)
{
	filename    = "./2_LightsaberSwing2.wav";
	description = AudioClosest3d;
	preload = true;
};

datablock AudioProfile(lightSaberSwingSound3)
{
	filename    = "./2_LightsaberSwing3.wav";
	description = AudioClosest3d;
	preload = true;
};

datablock AudioProfile(lightSaberPlayerHitSound1)
{
	filename    = "./LightSaberPlayer_Hit1.wav";
	description = AudioClosest3d;
	preload = true;
};

datablock AudioProfile(lightSaberPlayerHitSound2)
{
	filename    = "./LightSaberPlayer_Hit2.wav";
	description = AudioClosest3d;
	preload = true;
};

datablock AudioProfile(lightSaberPlayerHitSound3)
{
	filename    = "./LightSaberPlayer_Hit3.wav";
	description = AudioClosest3d;
	preload = true;
};

datablock AudioProfile(lightSaberPlayerHitSound4)
{
	filename    = "./LightSaberPlayer_Hit4.wav";
	description = AudioClosest3d;
	preload = true;
};

datablock AudioProfile(lightSaberZapSound)
{
	filename    = "./scorch1.wav";
	description = AudioClosest3d;
	preload = true;
};

datablock AudioProfile(lightSaberHumSound)
{
	fileName = "./LightsaberHum.wav";
	description = AudioCloseLooping3d;
	preload = 1;
};


//effects
datablock ParticleData(lightSaberExplosionParticle)
{
	dragCoefficient      = 2;
	gravityCoefficient   = 1.0;
	inheritedVelFactor   = 0.2;
	constantAcceleration = 0.0;
	spinRandomMin = -90;
	spinRandomMax = 90;
	lifetimeMS           = 150;
	lifetimeVarianceMS   = 100;
	textureName          = "./spark";
	colors[0]     = "0.9 0.9 1 0.9";
	colors[1]     = "0 0 1 0.0";
	sizes[0]      = 0.55;
	sizes[1]      = 0.5;
};

datablock ParticleEmitterData(lightSaberExplosionEmitter)
{
	ejectionPeriodMS = 7;
	periodVarianceMS = 0;
	ejectionVelocity = 12;
	velocityVariance = 1.0;
	ejectionOffset   = 0.0;
	thetaMin         = 0;
	thetaMax         = 45;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;
	orientParticles  = true;
	particles = "lightSaberExplosionParticle";

	uiName = "Ligthsaber spark";
};

datablock ExplosionData(lightSaberExplosion)
{
	//explosionShape = "";
	lifeTimeMS = 500;

	soundProfile = "";

	particleEmitter = lightSaberExplosionEmitter;
	particleDensity = 10;
	particleRadius = 0.2;

	faceViewer     = true;
	explosionScale = "1 1 1";

	shakeCamera = true;
	camShakeFreq = "20.0 22.0 20.0";
	camShakeAmp = "1.0 1.0 1.0";
	camShakeDuration = 0.5;
	camShakeRadius = 10.0;

	// Dynamic light
	lightStartRadius = 3;
	lightEndRadius = 0;
	lightStartColor = "0.2 0.2 0.6";
	lightEndColor = "0 0 0";
};


datablock ParticleData(lightSaberScorchExplosionParticle)
{
	dragCoefficient      = 5;
	gravityCoefficient   = -0.0;
	inheritedVelFactor   = 1.0;
	constantAcceleration = 0.0;
	lifetimeMS           = 100;
	lifetimeVarianceMS   = 0;
	textureName          = "base/data/particles/dot";
	spinSpeed           = 0.0;
	spinRandomMin        = 0.0;
	spinRandomMax        = 0.0;
	colors[0]     = "1 1 0 0.9";
	colors[1]     = "1 0 0 0.5";
	colors[2]     = "1 0 0 0";

	sizes[0]      = 0.15;
	sizes[1]      = 0.15;
	sizes[2]      = 0.3;

	times[0] = 0.0;
	times[1] = 0.5;
	times[2] = 1.0;

	useInvAlpha = false;
};

datablock ParticleEmitterData(lightSaberScorchExplosionEmitter)
{
	ejectionPeriodMS = 10;
	periodVarianceMS = 0;
	ejectionVelocity = 1;
	velocityVariance = 0.5;
	ejectionOffset   = 0;
	thetaMin         = 0;
	thetaMax         = 180;
	phiReferenceVel  = 0;
	phiVariance      = 360;
	overrideAdvance = false;
	particles = "lightSaberScorchExplosionParticle";

	uiName = "Ligthsaber Scorch";
};

datablock ExplosionData(lightSaberScorchExplosion)
{
	//explosionShape = "";
	lifeTimeMS = 500;

	soundProfile = "";

	particleEmitter = lightSaberScorchExplosionEmitter;
	particleDensity = 10;
	particleRadius = 0.2;

	faceViewer     = true;
	explosionScale = "1 1 1";

	shakeCamera = false;
	camShakeFreq = "20.0 22.0 20.0";
	camShakeAmp = "1.0 1.0 1.0";
	camShakeDuration = 0.5;
	camShakeRadius = 10.0;

	// Dynamic light
	lightStartRadius = 0;
	lightEndRadius = 0;
	lightStartColor = "1 1 0";
	lightEndColor = "0 0 0";
};

//projectile
datablock ProjectileData(lightSaberScorchProjectile)
{
	explosion = lightSaberScorchExplosion;
};

function lightSaberScorchProjectile::onExplode(%this, %obj, %pos)
{
	ServerPlay3D(lightSaberZapSound, %pos);
}

datablock ProjectileData(lightSaberProjectile)
{
	explosion = lightSaberExplosion;
};

function lightSaberProjectile::onExplode(%this, %obj, %pos)
{
	ServerPlay3D(lightSaberHitSound, %pos);
}

datablock ProjectileData(lightSaberBloodProjectile)
{
	explosion = MeleeBloodExplosion;
};

function lightSaberBloodProjectile::onExplode(%this, %obj, %pos)
{
	ServerPlay3D(lightSaberPlayerHitSound @ getRandom(1, 4), %pos);
}

//////////
// item //
//////////
datablock ItemData(lightSaberItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	 // Basic Item Properties
	shapeFile = "./lightSaber.dts";
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Lightsaber";
	iconName = "./icon_lightSaber";
	doColorShift = true;
	colorShiftColor = "0 0 1 1";

	 // Dynamic properties defined by the scripts
	image = lightSaberImage;
	canDrop = true;
};

////////////////
//weapon image//
////////////////
datablock ShapeBaseImageData(lightSaberImage)
{
	// Basic Item properties
	shapeFile = "./lightSaber.dts";
	emap = true;

	// Specify mount point & offset for 3rd person, and eye offset
	// for first person rendering.
	mountPoint = 0;
	offset = "0 0 0";

	// When firing from a point offset from the eye, muzzle correction
	// will adjust the muzzle vector to point to the eye LOS point.
	// Since this weapon doesn't actually fire from the muzzle point,
	// we need to turn this off.  
	correctMuzzleVector = false;

	eyeOffset = "0 0 0";

	// Add the WeaponImage namespace as a parent, WeaponImage namespace
	// provides some hooks into the inventory system.
	className = "WeaponImage";

	// Projectile && Ammo.
	item = lightSaberItem;
	ammo = " ";
	projectile = lightSaberScorchProjectile;
	projectileType = Projectile;

	//melee particles shoot from eye node for consistancy
	melee = true;

	//Special melee hitreg system
	directDamage = 65;

	meleeEnabled = true;
	meleeStances = true; //Use stance system?
	meleeCanClash = true; //If stances are enabled, can it clash?
	meleeTick = 24; //The speed of schedule loop in MS. Change this to animation FPS

	meleeRayLength = 2;

	meleeHitProjectile = lightSaberScorchProjectile;
	meleeBlockedProjectile = lightSaberProjectile;
	meleeHitPlayerProjectile = lightSaberBloodProjectile;

	meleePierceTerrain = true; //If we hit terrain hitreg will still go on until it hits a player
	meleeSingleHitProjectile = false; //If pierce terrain is on, set this to true so it doesn't spam hit projectiles

	meleeBlockedVelocity = 7;
	meleeBlockedStunTime = 0.600; //Length of stun in seconds

	//raise your arm up or not
	armReady = true;

	//casing = " ";
	doColorShift = lightSaberItem.doColorShift;
	colorShiftColor = lightSaberItem.colorShiftColor;

	// Images have a state system which controlightSaber how the animations
	// are run, which sounds are played, script callbacks, etc. This
	// state system is downloaded to the client so that clients can
	// predict state changes and animate accordingly.  The following
	// system supports basic ready->fire->reload transitions as
	// well as a no-ammo->dryfire idle state.

	// Initial start up state
	stateName[0]                     = "Activate";
	stateTimeoutValue[0]             = 0.5;
	stateTransitionOnTimeout[0]      = "Ready";
	stateSound[0]                    = lightSaberDrawSound;

	stateName[1]                     = "Ready";
	stateTransitionOnTriggerDown[1]  = "Fire";
	stateAllowImageChange[1]         = true;
	stateTransitionOnNotLoaded[1]      = "noAmmo";

	stateName[2]                    = "Fire";
	stateTransitionOnTimeout[2]     = "StopFire";
	stateTimeoutValue[2]            = 0.24;
	stateFire[2]                    = true;
	stateAllowImageChange[2]        = false;
	stateScript[2]                  = "onFire";
	stateWaitForTimeout[2]		       = true;

	stateName[3]                    = "StopFire";
	stateTransitionOnTriggerUp[3]   = "Ready";
	stateAllowImageChange[3]        = false;
	stateScript[3]                  = "onStopFire";

	stateName[4]                    = "noAmmo";
	stateTransitionOnLoaded[4]        = "Ready";
	stateAllowImageChange[4]        = false;
	stateScript[4]                  = "onNoAmmo";
};

function lightSaberImage::onMount(%this, %obj, %slot)
{
	%obj.playThread(2, tswing @ (%obj.swingPhase + 1) % 2 + (%obj.meleeStance ? 3 : 1));
	%obj.schedule(32, stopThread, 2);
	%obj.playAudio(2, lightSaberHumSound);
}

function lightSaberImage::onUnMount(%this, %obj, %slot)
{
	%obj.stopAudio(2);
	%obj.playThread(2, root);
}

function lightSaberImage::onStanceSwitch(%this, %obj, %slot)
{
	if (%obj.getImageState(%slot) !$= "Ready") return;
	%obj.meleeStance = (%obj.meleeStance + 1) % 2;
	%this.stopMeleeHitregLoop(%obj, %slot);
	%obj.playThread(2, tswing @ (%obj.swingPhase + 1) % 2 + (%obj.meleeStance ? 3 : 1));
	%obj.schedule(0, stopThread, 2);
	if (isobject(%client=%obj.client)) %client.centerPrint("\c4Switched to " @ (%obj.meleeStance ? "horizontal" : "vertical") @ " stance.", 2);
	%obj.playThread(3, plant);
	serverPlay3D(WeaponSwitchSound, %obj.getSlotTransform(0));
}

function lightSaberImage::onFire(%this, %obj, %slot)
{
	%obj.swingPhase = (%obj.swingPhase + 1) % 2;
	%obj.playthread(2, tswing @ %obj.swingPhase + (%obj.meleeStance ? 3 : 1));
	%this.MeleeHitregLoop(%obj, %slot, 12);
	serverPlay3d(lightSaberSwingSound @ getRandom(1, 3), %obj.getSlotTransform(%slot));
}

//dumb attempt at making projectile deflection below
package lightSaberPackage
{
	function projectileData::damage(%this, %obj, %col, %pos, %fade, %normal)
	{
		parent::damage(%this, %obj, %col, %pos, %fade, %normal);
	}
	function projectileData::onCollision(%this, %obj, %col, %pos, %fade, %normal)
	{
		if (%col.getType() & $TypeMasks::PlayerObjectType && %col.getMountedImage(0) == lightSaberImage.getID() && %col.activeSwing)
		{
			%obj.damageCancel[%col] = 1;
			%scaleFactor = getWord(%obj.getScale(), 2);
			%pos = %col.getHackPosition();

			%mask =
					$TypeMasks::FxBrickObjectType |
					$TypeMasks::TerrainObjectType |
					$TypeMasks::StaticShapeObjectType |
					$TypeMasks::VehicleObjectType |
					$TypeMasks::PlayerObjectType;

			//Adjust it so it aims towards cursor
			%correct = getWords(%col.getForwardVector(), 0, 1) SPC getWord(%col.getEyeVector(), 2);
			%b = vectorAdd(%col.getEyePoint(), vectorScale(%correct, 100));
			%ray = containerRayCast(%col.getEyePoint(), %b, %mask, %col);
			if (%ray)
			{
				%b = getWords(%ray, 1, 3);
				%c = vectorNormalize(vectorSub(%b, %pos));
			}
			else
				%c = %correct;
			%vel = vectorScale(%c, vectorLen(%obj.getVelocity()));
			//%vel = vectorAdd(%vec,vectorScale(%col.getVelocity(),%this.velInheritFactor));
			%p = new Projectile()
			{
				dataBlock = %this;
				initialPosition = %pos;
				initialVelocity = %vel;
				sourceObject = %col;
				client = %col.client;
				sourceSlot = 0;
				originPoint = %pos;
				reflectTime = getSimTime();
			};
			MissionCleanup.add(%p);
			%p.setScale(%scaleFactor SPC %scaleFactor SPC %scaleFactor);
			%obj.delete();
			return;
		}

		parent::onCollision(%this, %obj, %col, %pos, %fade, %normal);
	}
	function ProjectileData::radiusDamage(%this, %obj, %col, %distanceFactor, %pos, %damageAmt)
	{
		if(%obj.damageCancel[%col])
			return;

		return Parent::radiusDamage(%this, %obj, %col, %distanceFactor, %pos, %damageAmt);
	}
};
activatePackage(lightSaberPackage);