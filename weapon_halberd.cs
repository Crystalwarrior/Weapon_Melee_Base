datablock ItemData(HalberdItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	// Basic Item Properties
	shapeFile = "./Halberd.dts";
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Halberd";
	iconName = "./icon_sword";
	doColorShift = true;
	colorShiftColor = "0.471 0.471 0.471 1.000";

	// Dynamic properties defined by the scripts
	image = HalberdImage;
	canDrop = true;

	itemPropsClass = "MeleeWeaponProps";
	customPickupAlways = false;
	customPickupMultiple = false;

	durability = 1000; //hits until weapon breaks
};

////////////////
//weapon image//
////////////////
datablock ShapeBaseImageData(HalberdImage)
{
	// Basic Item properties
	shapeFile = "./Halberd.dts";
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
	item = HalberdItem;
	ammo = " ";
	projectile = MeleeClaymoreProjectile;
	projectileType = Projectile;

	//melee particles shoot from eye node for consistancy
	melee = true;

	//For shields
	twoHanded = true;

	//Special melee hitreg system
	directDamage = 35;

	armorPenetration = 0.2;
	
	meleeEnabled = true;
	meleeStances = false; //Use stance system?
	meleeCanClash = true; //If stances are enabled, can it clash? Keep this on if you want dagger to clash it
	meleeTick = 24; //The speed of schedule loop in MS. Change this to animation FPS
	meleeTracerCount = 2; //Amount of "tracer raycasts" for better hit detection. Note that this is better for wide swings as opposed to stabs.
	meleeTipFactor = 0.4; //Where the "tip" starts
	meleeTipDamage = true; //Only use tip to damage

	meleeRayLength = 2.8;

	meleeHitProjectile = MeleeClaymoreProjectile;
	meleeBlockedProjectile = MeleeClaymoreBlockProjectile;
	meleeHitPlayerProjectile = SwordBloodProjectile;

	meleePierceTerrain = false; //If we hit terrain hitreg will still go on until it hits a player
	meleeSingleHitProjectile = false; //If pierce terrain is on, set this to true so it doesn't spam hit projectiles
	meleeCanPierce = false; //All attacks pierce multiple targets

	meleeBlockedVelocity = 9;
	meleeBlockedStunTime = 1; //Length of stun in seconds (for self)

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
	stateTimeoutValue[0]             = 1;
	stateTransitionOnTimeout[0]      = "Ready";
	stateSound[0]                    = MeleeSwordDrawSound;

	stateName[1]                     = "Ready";
	stateTransitionOnTriggerDown[1]  = "CheckCharge";
	stateAllowImageChange[1]         = true;
	stateWaitForTimeout[1]			= false;
	stateTransitionOnNotLoaded[1]    = "noAmmo";

	stateName[2]                    = "Fire";
	stateTransitionOnTimeout[2]     = "Ready";
	stateTimeoutValue[2]            = 0.6;
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

function HalberdImage::onMount(%this, %obj, %slot)
{
    parent::onMount(%this, %obj, %slot);
	%obj.playThread(2, spearswing @ (%obj.swingPhase + 1) % 2 + 1);
	%obj.schedule(32, stopThread, 2);
}

function HalberdImage::onFire(%this, %obj, %slot)
{
	%this.setChargeSlowdown(%obj, 0);
	%obj.swingPhase = (%obj.swingPhase + 1) % 2;
	%obj.playthread(2, spearswing @ %obj.swingPhase + 1);
	%this.schedule(16, MeleeHitregLoop, %obj, %slot, 8);
	%obj.playAudio(2, HalberdSwingSound @ getRandom(1, 3));
}
function HalberdImage::onCheckCharge(%this, %obj, %slot)
{
	%this.setChargeSlowdown(%obj, 1);
}
function HalberdImage::onCharge(%this, %obj, %slot)
{
	%obj.playthread(2, spearswing @ (%obj.swingPhase + 1) % 2 + 3);
	%obj.schedule(0, stopThread, 2);
	%obj.playThread(3, plant);
	serverPlay3D(MeleeChargeSound, %obj.getSlotTransform(%slot));
	%obj.doChargeEmitter(%obj.getSlotTransform(%slot));
}

function HalberdImage::onChargeFire(%this, %obj, %slot)
{
	%this.setChargeSlowdown(%obj, 0);
	%obj.swingPhase = (%obj.swingPhase + 1) % 2;
	%obj.playthread(2, spearswing @ %obj.swingPhase + 3);
	%obj.chargeAttack = true;
	%this.schedule(150, MeleeHitregLoop, %obj, %slot, 12, 60);
	%obj.swingSchedule = %obj.schedule(150, playAudio, 2, maulSwingSound @ %obj.swingPhase + 1);
}