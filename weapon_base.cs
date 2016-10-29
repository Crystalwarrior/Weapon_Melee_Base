package MeleeBasePackage
{
	//function WeaponImage::onFire(%this, %obj, %slot)
	//{
	//	%obj.swingPhase = (%obj.swingPhase + 1) % 2;
	//	%obj.playthread(2, %this.meleeAnimation @ %obj.swingPhase + (%obj.meleeStance ? 3 : 1));
	//	%this.MeleeHitregLoop(%obj, %slot, 12);
	//}

	function Armor::onTrigger(%data, %this, %trig, %tog)
	{
		Parent::onTrigger(%data, %this, %trig, %tog);
		%image = %this.getMountedImage(0);
		if (!isObject(%image) || !%image.meleeEnabled || !%image.meleeStances)
			return;
		if (%trig == 4 && %tog)
		{
			%image.onStanceSwitch(%this, 0);
		}

		//if (%trig == 0 && %tog)
		//{
			//%this.swingPhase = 1;
		//	%this.meleeStance = 0;
		//	%image.stopMeleeHitregLoop(%this, 0);
			//%this.playThread(2, tswing @ (%this.meleeStance ? 3 : 1));
			//%this.schedule(0, stopThread, 2);
		//}
		//if (%trig == 3)
		//{
		//	if (%tog)
		//	{
		//		%this.setArmThread(land);
		//	}
		//	else
		//	{
		//		%this.setArmThread(look);
		//	}
		//}
	}
};

activatePackage("MeleeBasePackage");

function WeaponImage::onStanceSwitch(%this, %obj, %slot)
{
}

function WeaponImage::stopMeleeHitregLoop(%this, %obj, %slot)
{
	cancel(%obj.MeleeHitregLoop);
	if (isObject(%obj.line))
		%obj.line.delete();
	if (isObject(%obj.line2))
		%obj.line2.delete();
	%obj.activeSwing = 0;
}

function WeaponImage::MeleeCheckClash(%this, %obj, %slot, %col)
{
	%targImg = %col.getMountedImage(%slot);
	return %this.meleeCanClash && isObject(%targImg) && %targImg.meleeStances && %targImg.meleeCanClash && %obj.meleeStance == !%col.meleeStance && %col.activeSwing;
}

function WeaponImage::MeleeDamage(%this, %obj, %slot, %col, %damage, %pos)
{
	if (%damage $= "")
		%damage = %this.directDamage;
	%col.damage(%obj, %pos, %damage, $DamageType::Sword);
}

function WeaponImage::MeleeHitregLoop(%this, %obj, %slot, %frames, %damage, %last, %hitterrain)
{
	cancel(%obj.MeleeHitregLoop);
	if (%frames <= 0)
	{
		%this.stopMeleeHitregLoop(%obj, %slot);
		return;
	}
			
	if (!isObject(%obj) || %obj.getState() $= "Dead" || %obj.getMountedImage(%slot) != %this.getID())
	{
		%this.stopMeleeHitregLoop(%obj, %slot);
		return;
	}
	%obj.activeSwing = 1;
	//if (!isObject(%obj.line))
	//		%obj.line = createShape(CubeGlowShapeData, "1 1 1 0.5");
	//if (!isObject(%obj.line2))
	//		%obj.line2 = createShape(CubeGlowShapeData, "1 1 1 1");

	%a = getWords(%obj.getSlotTransform(%slot), 0, 2);
	%b = %obj.getMuzzlePoint(%slot);

	%vec = vectorSub(%b, %a);
	%vec = vectorScale(vectorNormalize(%vec), %this.meleeRayLength);//1.73895
	%b = vectorAdd(%a, %vec);

	//%obj.line.transformLine(%a, %b, 0.1);

	%mask =
			$TypeMasks::FxBrickObjectType |
			$TypeMasks::TerrainObjectType |
			$TypeMasks::StaticShapeObjectType |
			$TypeMasks::VehicleObjectType |
			$TypeMasks::PlayerObjectType;

	%ray = containerRayCast(%a, %b, %mask, %obj);
	if(%last $= "") %last = %b;
	%ray2 = containerRayCast(%last, %b, %mask, %obj);
	//%obj.line2.transformLine(%last, %b, 0.1);

	if (%ray || %ray2)
	{
		if(%ray2)
			%ray = %ray2;
		%position = getWords(%ray, 1, 3);
		%datablock = %this.meleeHitProjectile;

		if(miniGameCanDamage(%obj, %ray) != 0 && isFunction(%ray.getClassName(), "damage"))
		{
			%targImg = %ray.getMountedImage(%slot);
			if (%this.meleeStances && %this.MeleeCheckClash(%obj, %slot, %ray))
			{
				%start = %obj.getPosition();
				%end = %ray.getPosition();
				%velmult = %this.meleeBlockedVelocity;
				%ray.setVelocity(vectorAdd(vectorScale(vectorNormalize(vectorSub(%end, %start)), %velmult), "0 0 3"));
				%obj.setVelocity(vectorAdd(vectorScale(vectorNormalize(vectorSub(%start, %end)), %velmult), "0 0 3"));
				%ray.setImageLoaded(%slot, 0);
				%obj.setImageLoaded(%slot, 0);
				%targImg.stopMeleeHitregLoop(%ray, %slot);
				%this.stopMeleeHitregLoop(%obj, %slot);
				%ray.stopThread(2);
				%obj.stopThread(2);
				%ray.swingPhase++;
				%obj.swingPhase++;
				%delay = %this.meleeBlockedStunTime * 1000;
				%ray.schedule(%delay, setImageLoaded, %slot, 1);
				%obj.schedule(%delay, setImageLoaded, %slot, 1);
				%datablock = %this.meleeBlockedProjectile;
			}
			else
			{
				%this.MeleeDamage(%obj, slot, %ray, %damage, %position);
				%datablock = %this.meleeHitPlayerProjectile;
			}
			%hitplayer = true;
		}

		if (isObject(%datablock) && (!%this.meleeSingleHitProjectile || %hitplayer || !%hitterrain))
		{
			%projectile = new Projectile()
			{
					datablock = %datablock;
					initialPosition = %position;
					initialVelocity = "0 0 0";
			};

			MissionCleanup.add(%projectile);

			%projectile.explode();
		}

		if (!%this.meleePierceTerrain || %hitplayer)
		{
			%this.stopMeleeHitregLoop(%obj, %slot);
			return;
		}
		%hitterrain = true;
	}
	%last = %b;
	%obj.MeleeHitregLoop = %this.schedule(%this.meleeTick, "MeleeHitregLoop", %obj, %slot, %frames--, %damage, %last, %hitterrain);
}

//vars on image to mod:
//
//meleeEnabled
//meleeStances
//meleeTick = 24
//
//meleeHitProjectile
//meleeBlockedProjectile
//meleeHitPlayerProjectile
//
//meleeBlockedVelocity
//meleeBlockedStunTime