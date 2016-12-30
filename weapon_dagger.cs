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

	//For shields
	twoHanded = true;

	//Special melee hitreg system
	directDamage = 20;

	meleeEnabled = true;
	meleeStances = true; //Use stance system?
	meleeCanClash = false; //If stances are enabled, can it clash?
	meleeTick = 24; //The speed of schedule loop in MS. Change this to animation FPS
	meleeTracerCount = 0; //Amount of "tracer raycasts" for better hit detection. Note that this is better for wide swings as opposed to stabs.

	meleeRayLength = 1.3;

	meleeHitProjectile = MeleeSharpProjectile;
	meleeBlockedProjectile = MeleeBlockProjectile;
	meleeHitPlayerProjectile = DaggerBloodProjectile;

	meleePierceTerrain = false; //If we hit terrain hitreg will still go on until it hits a player
	meleeSingleHitProjectile = false; //If pierce terrain is on, set this to true so it doesn't spam hit projectiles

	meleeBlockedVelocity = 1;
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
	stateTimeoutValue[0]             = 0.2; //fast asspull weapon
	stateTransitionOnTimeout[0]      = "Ready";
	stateSound[0]                    = ""; //No sound

	stateName[1]                     = "Ready";
	stateTransitionOnTriggerDown[1]  = "CheckCharge";
	stateAllowImageChange[1]         = true;
	stateWaitForTimeout[1]			= false;
	stateTransitionOnNotLoaded[1]    = "noAmmo";
	
	stateName[2]                    = "Fire";
	stateTransitionOnTimeout[2]     = "Ready";
	stateTimeoutValue[2]            = 0.35;
	stateFire[2]                    = true;
	stateAllowImageChange[2]        = false;
	stateScript[2]                  = "onFire";
	stateWaitForTimeout[2]		       = true;

	stateName[4]                     = "CheckStab";
	stateTransitionOnAmmo[4]		= "Fire";
	stateTransitionOnNoAmmo[4]		= "StabFire";

	stateName[5]                    = "StabFire";
	stateTransitionOnTimeout[5]     = "Ready";
	stateTimeoutValue[5]            = 0.4;
	stateFire[5]                    = true;
	stateAllowImageChange[5]        = false;
	stateScript[5]                  = "onFire";
	stateWaitForTimeout[5]		       = true;

	stateName[6]                    = "CheckCharge";
	stateTransitionOnTriggerUp[6]   = "CheckStab";
	stateTransitionOnTimeout[6]		= "ChargeReady";
	stateTimeoutValue[6]            = 0.3;
	stateAllowImageChange[6]        = false;
	stateWaitForTimeout[6]			= false;
	stateScript[6]                  = "onCheckCharge";

	stateName[7]                    = "ChargeReady";
	stateTransitionOnTriggerUp[7]   = "Fire"; //Forward grip when charged, always
	stateAllowImageChange[7]        = false;
	stateScript[7]                  = "onCharge";

	stateName[10]                    = "noAmmo";
	stateTransitionOnLoaded[10]        = "Ready";
	stateAllowImageChange[10]        = false;
	stateScript[10]                  = "onNoAmmo";
};

function DaggerImage::onMount(%this, %obj, %slot)
{
    parent::onMount(%this, %obj, %slot);
	if (%obj.meleeStance)
		%obj.playthread(2, stabdagger @ (%obj.swingPhase + 1) % 2 + 1);
	else
		%obj.playthread(2, swingdagger @ (%obj.swingPhase + 1) % 3 + 1);
	%obj.schedule(32, stopThread, 2);
	%obj.setImageAmmo(%slot, !%obj.meleeStance);
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
	if (%obj.meleeStance && !%obj.chargeAttack)
	{
		%obj.swingPhase = (%obj.swingPhase + 1) % 2;
		%obj.playthread(2, stabdagger @ %obj.swingPhase + 1);
		%damage = 25;
	}
	else
	{
		%obj.swingPhase = (%obj.swingPhase + 1) % 3;
		%obj.playthread(2, swingdagger @ %obj.swingPhase + 1);
		%damage = 20;
	}
	%obj.stopAudio(2);
	%obj.playAudio(2, DaggerSwingSound);
	%this.schedule(16, MeleeHitregLoop, %obj, %slot, 12, %damage);
}


function DaggerImage::onCharge(%this, %obj, %slot)
{
	%obj.playThread(3, plant);
	%obj.playthread(2, swingdagger @ (%obj.swingPhase + 1) % 3 + 1);
	%obj.schedule(0, stopThread, 2);
	%obj.chargeAttack = true;
	serverPlay3D(MeleeChargeSound, %obj.getSlotTransform(%slot));
	%obj.doChargeEmitter(%obj.getSlotTransform(%slot));
}

function DaggerImage::MeleeDamage(%this, %obj, %slot, %col, %damage, %pos)
{
	if (%damage $= "")
		%damage = %this.directDamage;
	%dot = vectorDot(%col.getForwardVector(), %obj.getForwardVector());
	if (%dot > 0)
	{
		%damage *= 2 + (1 * %obj.meleeStance);
		if(%col.isDowned)
			%damage = 200; //You're screwed, only 40% armor will leave you at 20HP downed.
	}
	%col.damage(%obj, %pos, %damage, $DamageType::Sword);
}

function DaggerImage::MeleeCheckClash(%this, %obj, %slot, %col)
{
	%targImg = %col.getMountedImage(%slot);
	%shieldImg = %col.getMountedImage(3);
	if(isObject(%shieldImg) && %shieldImg.isShield && getSimTime() - %col.lastShield <= $ShieldBlockTime) //We got a shielder here
	{
		if(%shieldImg.onBlock(%col, %slot, %obj))
			return true;
	}
	if (isObject(%targImg) && %targImg == DaggerImage.getID())
		return %obj.activeSwing && %obj.meleeStance == 0 && %col.activeSwing && %col.meleeStance == 0;
	if (%obj.chargeAttack && !%col.chargeAttack || !%obj.chargeAttack && %col.chargeAttack) //If you charge with dagger you can ONLY clash with enemy charge attack
		return false; //Charge attacks prevent clashing for everything
	return %obj.activeSwing && %obj.meleeStance == 0 && isObject(%targImg) && %targImg.meleeCanClash && %col.activeSwing;
}