datablock ItemData(ShortSwordItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	// Basic Item Properties
	shapeFile = "./sword.dts";
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Shortsword";
	iconName = "./icon_sword";
	doColorShift = true;
	colorShiftColor = "0.471 0.471 0.471 1.000";

	// Dynamic properties defined by the scripts
	image = ShortSwordImage;
	canDrop = true;

	itemPropsClass = "MeleeWeaponProps";
	customPickupAlways = false;
	customPickupMultiple = false;

	durability = 2500; //hits until weapon breaks
};

datablock ProjectileData(SwordBloodProjectile)
{
	explosion = MeleeBloodExplosion;
};

function SwordBloodProjectile::onExplode(%this, %obj, %pos)
{
	ServerPlay3D(MeleeSlashSound @ getRandom(1, 4), %pos);
}

////////////////
//weapon image//
////////////////
datablock ShapeBaseImageData(ShortSwordImage)
{
	// Basic Item properties
	shapeFile = "./sword.dts";
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
	item = ShortSwordItem;
	ammo = " ";
	projectile = MeleeSharpProjectile;
	projectileType = Projectile;

	//melee particles shoot from eye node for consistancy
	melee = true;

	//Special melee hitreg system
	directDamage = 34;
	armorPenetration = 0.1;

	meleeEnabled = true;
	meleeStances = true; //Use stance system?
	meleeCanClash = true; //If stances are enabled, can it clash? Keep this on if you want dagger to clash it
	meleeTick = 24; //The speed of schedule loop in MS. Change this to animation FPS
	meleeTracerCount = 2; //Amount of "tracer raycasts" for better hit detection. Note that this is better for wide swings as opposed to stabs.

	meleeRayLength = 1.73895;

	meleeHitProjectile = MeleeSharpProjectile;
	meleeBlockedProjectile = MeleeBlockProjectile;
	meleeHitPlayerProjectile = SwordBloodProjectile;

	meleePierceTerrain = false; //If we hit terrain hitreg will still go on until it hits a player
	meleeSingleHitProjectile = false; //If pierce terrain is on, set this to true so it doesn't spam hit projectiles

	meleeBlockedVelocity = 7;
	meleeBlockedStunTime = 0.5; //Length of stun in seconds (for self)

	meleeBounceAnim[3] = "plant"; //Animation in [%slot] when hitting something
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
	stateTimeoutValue[0]             = 0.7;
	stateTransitionOnTimeout[0]      = "Ready";
	stateSound[0]                    = MeleeSwordDrawSound;

	stateName[1]                     = "Ready";
	stateTransitionOnTriggerDown[1]  = "CheckCharge";
	stateAllowImageChange[1]         = true;
	stateWaitForTimeout[1]			= false;
	stateTransitionOnNotLoaded[1]    = "noAmmo";

	stateName[2]                    = "Fire";
	stateTransitionOnTimeout[2]     = "Ready";
	stateTimeoutValue[2]            = 0.34;
	stateFire[2]                    = true;
	stateAllowImageChange[2]        = false;
	stateScript[2]                  = "onFire";
	stateWaitForTimeout[2]		       = true;

	stateName[3]                    = "CheckCharge";
	stateTransitionOnTriggerUp[3]   = "Fire";
	stateTransitionOnTimeout[3]		= "ChargeReady";
	stateTimeoutValue[3]            = 0.8;
	stateAllowImageChange[3]        = false;
	stateWaitForTimeout[3]			= false;
	stateScript[3]                  = "onCheckCharge";

	stateName[4]                    = "ChargeReady";
	stateTransitionOnTriggerUp[4]   = "ChargeFire";
	stateAllowImageChange[4]        = false;
	stateScript[4]                  = "onCharge";

	stateName[5]                    = "ChargeFire";
	stateTransitionOnTimeout[5]     = "Ready";
	stateTimeoutValue[5]            = 0.6;
	stateFire[5]                    = true;
	stateAllowImageChange[5]        = false;
	stateScript[5]                  = "onChargeFire";
	stateWaitForTimeout[5]		    = true;

	stateName[8]                    = "noAmmo";
	stateTransitionOnLoaded[8]      = "Ready";
	stateAllowImageChange[8]        = false;
	stateScript[8]                  = "onNoAmmo";
};

function ShortSwordImage::onMount(%this, %obj, %slot)
{
    parent::onMount(%this, %obj, %slot);
	%shieldCheck = isObject(%obj.getMountedImage(3)) && %obj.getMountedImage(3).isShield;
	%obj.playThread(2, tswing @ (%obj.swingPhase + 1) % 2 + (%obj.meleeStance ? 3 : 1));
	%obj.schedule(32, stopThread, 2);
}

function ShortSwordImage::onStanceSwitch(%this, %obj, %slot)
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

function ShortSwordImage::onFire(%this, %obj, %slot)
{
	%this.setChargeSlowdown(%obj, 0);
	%obj.swingPhase = (%obj.swingPhase + 1) % 2;
	%obj.playthread(2, tswing @ %obj.swingPhase + (%obj.meleeStance ? 3 : 1));
	%this.schedule(16, MeleeHitregLoop, %obj, %slot, 14);
	%obj.stopAudio(2);
	%obj.playAudio(2, ShortswordSwingSound @ getRandom(1, 3));
}
function ShortSwordImage::onCheckCharge(%this, %obj, %slot)
{
	%this.setChargeSlowdown(%obj, 1);
}
function ShortSwordImage::onCharge(%this, %obj, %slot)
{
	if(%obj.meleeStance)
		%obj.swingPhase++;
	%obj.swingPhase = %obj.swingPhase % 2;
	%seq = "2h" @ (!%obj.meleeStance ? "stab1" : ("swing" @ %obj.swingPhase+1));
	%shieldCheck = isObject(%obj.getMountedImage(3)) && %obj.getMountedImage(3).isShield;
	if(%shieldCheck)
		%seq = "bswing" @ %obj.swingPhase + 1;
	%obj.playthread(2, %seq);
	%obj.schedule(0, stopThread, 2);
	%obj.playThread(3, plant);
	serverPlay3D(MeleeChargeSound, %obj.getSlotTransform(%slot));
	%obj.doChargeEmitter(%obj.getSlotTransform(%slot));
}

function ShortSwordImage::onChargeFire(%this, %obj, %slot)
{
	%this.setChargeSlowdown(%obj, 0);
	%seq = "2h" @ (!%obj.meleeStance ? "stab1" : ("swing" @ %obj.swingPhase+1));
	%shieldCheck = isObject(%obj.getMountedImage(3)) && %obj.getMountedImage(3).isShield;
	if(%shieldCheck)
		%seq = "bswing" @ %obj.swingPhase + 1;
	%obj.playthread(2, %seq);
	%obj.chargeAttack = true;
	%frames = 12;
	%delay = 32;
	if(%obj.meleeStance || %shieldCheck)
	{
		%frames = 18;
		%delay = 200;
	}
	%this.schedule(16, MeleeHitregLoop, %obj, %slot, %frames, %obj.meleeStance ? 50 : 40); //slash VS stab
	%obj.swingSchedule = %obj.schedule(%delay, playAudio, 2, SwordSwingSound @ getRandom(1, 3));
}