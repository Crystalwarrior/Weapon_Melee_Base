datablock ItemData(DaggerItem)
{
	category = "Weapon";  // Mission editor category
	className = "Weapon"; // For inventory system

	// Basic Item Properties
	shapeFile = "./dagger.dts";
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	//gui stuff
	uiName = "Dagger";
	iconName = "./icon_sword";
	doColorShift = true;
	colorShiftColor = "0.471 0.471 0.471 1.000";

	// Dynamic properties defined by the scripts
	image = DaggerImage;
	canDrop = true;
};

datablock ProjectileData(DaggerBloodProjectile)
{
	explosion = MeleeBloodExplosion;
};

function DaggerBloodProjectile::onExplode(%this, %obj, %pos)
{
	ServerPlay3D(MeleeStabSound @ getRandom(1,5), %pos);
}

////////////////
//weapon image//
////////////////
datablock ShapeBaseImageData(DaggerImage)
{
	// Basic Item properties
	shapeFile = "./dagger.dts";
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
	item = DaggerItem;
	ammo = " ";
	projectile = MeleeSharpProjectile;
	projectileType = Projectile;

	//melee particles shoot from eye node for consistancy
	melee = true;

	//Special melee hitreg system
	directDamage = 10;

	meleeEnabled = true;
	meleeStances = true; //Use stance system?
	meleeCanClash = false; //If stances are enabled, can it clash?
	meleeTick = 24; //The speed of schedule loop in MS. Change this to animation FPS

	meleeRayLength = 1.3;

	meleeHitProjectile = MeleeSharpProjectile;
	meleeBlockedProjectile = MeleeBlockProjectile;
	meleeHitPlayerProjectile = DaggerBloodProjectile;

	meleePierceTerrain = false; //If we hit terrain hitreg will still go on until it hits a player
	meleeSingleHitProjectile = false; //If pierce terrain is on, set this to true so it doesn't spam hit projectiles

	meleeBlockedVelocity = 1;
	meleeBlockedStunTime = 0.500; //Length of stun in seconds

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
	stateTimeoutValue[0]             = 0.2; //fast asspull weapon
	stateTransitionOnTimeout[0]      = "Ready";
	stateSound[0]                    = ""; //No sound

	stateName[1]                     = "Ready";
	stateTransitionOnTriggerDown[1]  = "Fire";
	stateAllowImageChange[1]         = true;
	stateTransitionOnNotLoaded[1]    = "noAmmo";
	stateTransitionOnNoAmmo[1]		 = "StabReady";

	stateName[2]                    = "Fire";
	stateTransitionOnTimeout[2]     = "CheckTrigger";
	stateTimeoutValue[2]            = 0.3;
	stateFire[2]                    = true;
	stateAllowImageChange[2]        = false;
	stateScript[2]                  = "onFire";
	stateWaitForTimeout[2]		       = true;

	stateName[3]					= "CheckTrigger";
	stateTransitionOnTriggerUp[3]	= "Ready";
	stateTransitionOnTriggerDown[3]	= "Fire";
	stateTransitionOnNoAmmo[3]		 = "StabReady";
	stateTransitionOnNotLoaded[3]    = "noAmmo";
	stateAllowImageChange[3]        = false;

	stateName[4]                    = "StopFire";
	stateTransitionOnTriggerUp[4]   = "Ready";
	stateAllowImageChange[4]        = false;
	stateScript[4]                  = "onStopFire";

	stateName[5]                     = "StabReady";
	stateTransitionOnTriggerDown[5]  = "StabFire";
	stateAllowImageChange[5]         = true;
	stateTransitionOnNotLoaded[5]    = "noAmmo";
	stateTransitionOnAmmo[5]		 = "Ready";

	stateName[6]                    = "StabFire";
	stateTransitionOnTimeout[6]     = "StopFire";
	stateTimeoutValue[6]            = 0.4;
	stateFire[6]                    = true;
	stateAllowImageChange[6]        = false;
	stateScript[6]                  = "onFire";
	stateWaitForTimeout[6]		       = true;

	stateName[7]                    = "noAmmo";
	stateTransitionOnLoaded[7]        = "Ready";
	stateAllowImageChange[7]        = false;
	stateScript[7]                  = "onNoAmmo";
};

function DaggerImage::onMount(%this, %obj, %slot)
{
	if (%obj.meleeStance)
		%obj.playthread(2, stabdagger @ (%obj.swingPhase + 1) % 2 + 1);
	else
		%obj.playthread(2, swingdagger @ (%obj.swingPhase + 1) % 3 + 1);
	%obj.schedule(32, stopThread, 2);
	%obj.setImageAmmo(%slot, 1);
}

function DaggerImage::onStanceSwitch(%this, %obj, %slot)
{
	//if (%obj.getImageState(%slot) !$= "Ready") return;
	%obj.meleeStance = (%obj.meleeStance + 1) % 2;
	%this.stopMeleeHitregLoop(%obj, %slot);

	if (%obj.meleeStance)
		%obj.playthread(2, stabdagger @ (%obj.swingPhase + 1) % 2 + 1);
	else
		%obj.playthread(2, swingdagger @ (%obj.swingPhase + 1) % 3 + 1);

	%obj.schedule(0, stopThread, 2);
	if (isobject(%client=%obj.client)) %client.centerPrint("\c4Switched to " @ (%obj.meleeStance ? "reverse" : "forward") @ " grip.", 2);
	%obj.playThread(3, plant);
	%obj.setImageAmmo(%slot, !%obj.meleeStance);
	//serverPlay3D(WeaponSwitchSound, %this.getSlotTransform(0));
}

function DaggerImage::onFire(%this, %obj, %slot)
{
	if (%obj.meleeStance)
	{
		%obj.swingPhase = (%obj.swingPhase + 1) % 2;
		%obj.playthread(2, stabdagger @ %obj.swingPhase + 1);
		%damage = 20;
	}
	else
	{
		%obj.swingPhase = (%obj.swingPhase + 1) % 3;
		%obj.playthread(2, swingdagger @ %obj.swingPhase + 1);
		%damage = 15;
	}
	
	%this.MeleeHitregLoop(%obj, %slot, 12, %damage);
}

function DaggerImage::MeleeDamage(%this, %obj, %slot, %col, %damage, %pos)
{
	if (%damage $= "")
		%damage = %this.directDamage;
	%dot = vectorDot(%col.getForwardVector(), %obj.getForwardVector());
	if (%dot > 0)
	{
		%damage *= 3 + (2 * %obj.meleeStance); //Reverse grip deals 4x damage + %dot, so almost 5x
	}
	%col.damage(%obj, %pos, %damage, $DamageType::Sword);
}

function DaggerImage::MeleeCheckClash(%this, %obj, %slot, %col)
{
	%targImg = %col.getMountedImage(%slot);
	if (isObject(%targImg) && %targImg == DaggerImage.getID())
		return %obj.meleeStance == 0 && %col.activeSwing && %col.meleeStance == 0;
	return %obj.meleeStance == 0 && isObject(%targImg) && %targImg.meleeStances && %targImg.meleeCanClash && %col.activeSwing;
}