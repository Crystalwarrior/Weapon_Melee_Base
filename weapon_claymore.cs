datablock ItemData(ClaymoreItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	// Basic Item Properties
	shapeFile = "./claymore.dts";
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Claymore";
	iconName = "./icon_sword";
	doColorShift = true;
	colorShiftColor = "0.471 0.471 0.471 1.000";

	// Dynamic properties defined by the scripts
	image = ClaymoreImage;
	canDrop = true;

	itemPropsClass = "MeleeWeaponProps";
	customPickupAlways = false;
	customPickupMultiple = false;

	durability = 3000; //hits until weapon breaks
};

datablock AudioProfile(MeleeClaymoreHitSound1)
{
	filename	= "./sounds/large_sword_Blocked_01.wav";
	description	= AudioClosest3d;
	preload		= true;
};
datablock AudioProfile(MeleeClaymoreHitSound2)
{
	filename	= "./sounds/large_sword_Blocked_02.wav";
	description	= AudioClosest3d;
	preload		= true;
};
datablock AudioProfile(MeleeClaymoreHitSound3)
{
	filename	= "./sounds/large_sword_Blocked_03.wav";
	description	= AudioClosest3d;
	preload		= true;
};
datablock AudioProfile(MeleeClaymoreHitSound4)
{
	filename	= "./sounds/large_sword_Blocked_04.wav";
	description	= AudioClosest3d;
	preload		= true;
};
datablock AudioProfile(MeleeClaymoreHitSound5)
{
	filename	= "./sounds/large_sword_Blocked_05.wav";
	description	= AudioClosest3d;
	preload		= true;
};
datablock AudioProfile(MeleeClaymoreHitSound6)
{
	filename	= "./sounds/large_sword_Blocked_06.wav";
	description	= AudioClosest3d;
	preload		= true;
};

datablock AudioProfile(MeleeClaymoreBlockSound1)
{
	filename	= "./sounds/large_sword_blocking_01.wav";
	description	= AudioClosest3d;
	preload		= true;
};
datablock AudioProfile(MeleeClaymoreBlockSound2)
{
	filename	= "./sounds/large_sword_blocking_02.wav";
	description	= AudioClosest3d;
	preload		= true;
};
datablock AudioProfile(MeleeClaymoreBlockSound3)
{
	filename	= "./sounds/large_sword_blocking_03.wav";
	description	= AudioClosest3d;
	preload		= true;
};

datablock ProjectileData(MeleeClaymoreProjectile)
{
	explosion = MeleeSharpExplosion;
};

function MeleeClaymoreProjectile::onExplode(%this, %obj, %pos)
{
	ServerPlay3D(MeleeClaymoreHitSound @ getRandom(1, 6), %pos);
}

datablock ProjectileData(MeleeClaymoreBlockProjectile)
{
	explosion = BladeSmallMetalExplosion;
};
function MeleeClaymoreBlockProjectile::onExplode(%this, %obj, %pos)
{
	ServerPlay3D(MeleeClaymoreBlockSound @ getRandom(1, 3), %pos);
}


////////////////
//weapon image//
////////////////
datablock ShapeBaseImageData(ClaymoreImage)
{
	// Basic Item properties
	shapeFile = "./claymore.dts";
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
	item = ClaymoreItem;
	ammo = " ";
	projectile = MeleeClaymoreProjectile;
	projectileType = Projectile;

	//melee particles shoot from eye node for consistancy
	melee = true;

	//For shields
	twoHanded = true;

	//Special melee hitreg system
	directDamage = 40;

	desiredSlowdown = 1; //How much slowdown is applied when this is wielded. Don't go above that limit, though.

	meleeEnabled = true;
	meleeStances = false; //Use stance system?
	meleeCanClash = true; //If stances are enabled, can it clash? Keep this on if you want dagger to clash it
	meleeTick = 24; //The speed of schedule loop in MS. Change this to animation FPS
	meleeTracerCount = 2; //Amount of "tracer raycasts" for better hit detection. Note that this is better for wide swings as opposed to stabs.

	meleeRayLength = 2.35;

	meleeHitProjectile = MeleeClaymoreProjectile;
	meleeBlockedProjectile = MeleeClaymoreBlockProjectile;
	meleeHitPlayerProjectile = AxeBloodProjectile;

	meleePierceTerrain = false; //If we hit terrain hitreg will still go on until it hits a player
	meleeSingleHitProjectile = false; //If pierce terrain is on, set this to true so it doesn't spam hit projectiles
	meleeCanPierce = true; //All attacks pierce multiple targets

	meleeBlockedVelocity = 7;
	meleeBlockedStunTime = 1; //Length of stun in seconds (for self)

	meleeBounceAnim[3] = "shiftAway"; //Animation in [%slot] when hitting something
	meleeBounceAnim[1] = "leftRecoil";
	meleeBouncePlayer = false; //Whether or not bounce animation is played when you hit players - enable for blunt weapons

	//raise your arm up or not
	armReady = false;

	//casing = " ";
	doColorShift = true;
	colorShiftColor = "0.471 0.471 0.471 1.000";

	// Images have a state system which controls how the animations
	// are run, which sounds are played, script callbacks, etc. This
	// state system is downloaded to the client so that clients can
	// predict state changes and animate accordingly.  The following
	// system supports basic ready->fire->reload transitions as
	// well as a no-ammo->dryfire idle state.

	// Initial start up state
	stateName[0]                     = "Activate";
	stateTimeoutValue[0]             = 1.1;
	stateTransitionOnTimeout[0]      = "Ready";
	stateSound[0]                    = MeleeSwordDrawSound;

	stateName[1]                     = "Ready";
	stateTransitionOnTriggerDown[1]  = "CheckCharge";
	stateAllowImageChange[1]         = true;
	stateWaitForTimeout[1]			= false;
	stateTransitionOnNotLoaded[1]    = "noAmmo";

	stateName[2]                    = "Fire";
	stateTransitionOnTimeout[2]     = "Ready";
	stateTimeoutValue[2]            = 0.9;
	stateFire[2]                    = true;
	stateAllowImageChange[2]        = false;
	stateScript[2]                  = "onFire";
	stateWaitForTimeout[2]		       = true;

	stateName[3]                    = "CheckCharge";
	stateTransitionOnTriggerUp[3]   = "Fire";
	stateTransitionOnTimeout[3]		= "ChargeReady";
	stateTimeoutValue[3]            = 1;
	stateAllowImageChange[3]        = false;
	stateWaitForTimeout[3]			= false;
	stateScript[3]                  = "onCheckCharge";

	stateName[4]                    = "ChargeReady";
	stateTransitionOnTriggerUp[4]   = "ChargeFire";
	stateAllowImageChange[4]        = false;
	stateScript[4]                  = "onCharge";

	stateName[5]                    = "ChargeFire";
	stateTransitionOnTimeout[5]     = "Ready";
	stateTimeoutValue[5]            = 1.1;
	stateFire[5]                    = true;
	stateAllowImageChange[5]        = false;
	stateScript[5]                  = "onChargeFire";
	stateWaitForTimeout[5]		    = true;

	stateName[8]                    = "noAmmo";
	stateTransitionOnLoaded[8]      = "Ready";
	stateAllowImageChange[8]        = false;
	stateScript[8]                  = "onNoAmmo";
};

function ClaymoreImage::onMount(%this, %obj, %slot)
{
    parent::onMount(%this, %obj, %slot);
	%obj.playThread(2, "2hswing" @ (%obj.swingPhase + 1) % 2 + 1);
	%obj.schedule(32, stopThread, 2);
}

function ClaymoreImage::onFire(%this, %obj, %slot)
{
	%obj.swingPhase = (%obj.swingPhase + 1) % 2;
	%obj.playthread(2, "2hswing" @ %obj.swingPhase + 1);
	%this.schedule(64, MeleeHitregLoop, %obj, %slot, 18);
	%obj.swingSchedule = %obj.schedule(200, playAudio, 2, longswordSwingSound @ getRandom(1, 3));
}

function ClaymoreImage::onCharge(%this, %obj, %slot)
{
	%obj.playthread(2, "2hstab1");
	%obj.schedule(0, stopThread, 2);
	%obj.playThread(3, plant);
	serverPlay3D(MeleeChargeSound, %obj.getSlotTransform(%slot));
	%obj.doChargeEmitter(%obj.getSlotTransform(%slot));
}

function ClaymoreImage::onChargeFire(%this, %obj, %slot)
{
	%obj.swingPhase = 3; //Force a swing phase for later non-charge hits
	%obj.playthread(2, "2hstab1");
	//%obj.playThread(3, activate);
	%obj.chargeAttack = true;
	%this.schedule(200, MeleeHitregLoop, %obj, %slot, 18, 55);
	%obj.swingSchedule = %obj.schedule(50, playAudio, 2, maulSwingSound1);
}