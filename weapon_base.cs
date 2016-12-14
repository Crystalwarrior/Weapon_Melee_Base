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
	%obj.stopping = "";
	%obj.activeSwing = 0;
	%obj.lastSwingStop = getSimTime();
}

function WeaponImage::MeleeCheckClash(%this, %obj, %slot, %col)
{
	%targImg = %col.getMountedImage(%slot);
	return %obj.activeSwing && %this.meleeStances && %this.meleeCanClash && isObject(%targImg) && %targImg.meleeStances && %targImg.meleeCanClash && %obj.meleeStance == !%col.meleeStance && %col.activeSwing;
}

function WeaponImage::MeleeDamage(%this, %obj, %slot, %col, %damage, %pos)
{
	if (%damage $= "")
		%damage = %this.directDamage;
	%col.damage(%obj, %pos, %damage, $DamageType::Sword);
}

function WeaponImage::MeleeHitregLoop(%this, %obj, %slot, %frames, %damage, %pierce, %last, %hitterrain)
{
	cancel(%obj.MeleeHitregLoop);
	if (%frames == 0 || %frames $= "")
	{
		%this.stopMeleeHitregLoop(%obj, %slot);
		//talk("No frames, stopping");
		return;
	}

	if(%pierce $= "")
		%pierce = %this.meleeCanPierce;
			
	if (!isObject(%obj) || %obj.getState() $= "Dead" || %obj.getMountedImage(%slot) != %this.getID())
	{
		%this.stopMeleeHitregLoop(%obj, %slot);
		//talk("%obj is nonexistant/dead or image is not correct");
		return;
	}

	if (%obj.stopping) //failsafe to make sure clashes properly cancel attacks
	{
		%this.stopMeleeHitregLoop(%obj, %slot);
		//talk("Shouldn't damage, .stopping is on");
		return;
	}
	%obj.activeSwing = 1;
	if($ComplexDebug)
	{
		if (!isObject(%obj.line))
				%obj.line = createShape(CubeGlowShapeData, "1 1 1 0.5");
		if (!isObject(%obj.line2))
				%obj.line2 = createShape(CubeGlowShapeData, "1 1 1 1");
	}

	%a = getWords(%obj.getSlotTransform(%slot), 0, 2);
	%b = %obj.getMuzzlePoint(%slot);

	%scaleFactor = getWord(%obj.getScale(), 2);

	%vec = vectorSub(%b, %a);
	%vec = vectorScale(vectorNormalize(%vec), %this.meleeRayLength * %scaleFactor);//1.73895
	%b = vectorAdd(%a, %vec);

	if($ComplexDebug)
		%obj.line.transformLine(%a, %b, 0.1);

	%mask =
			$TypeMasks::FxBrickObjectType |
			$TypeMasks::TerrainObjectType |
			$TypeMasks::StaticShapeObjectType |
			$TypeMasks::VehicleObjectType |
			$TypeMasks::PlayerObjectType;

	%ray = containerRayCast(%a, %b, %mask, %obj);
	if(%last $= "") %last = %b;
	%ray2 = containerRayCast(%last, %b, %mask, %obj);
	if($ComplexDebug)
		%obj.line2.transformLine(%last, %b, 0.1);

	%swingspeed = vectorDist(%last, %b);
	if ((%ray || %ray2) && (!%this.meleeIsFreeform || %swingspeed >= 0.2))
	{
		if(%ray2)
			%ray = %ray2;

		%position = getWords(%ray, 1, 3);
		%datablock = %this.meleeHitProjectile;

		if(miniGameCanDamage(%obj, %ray) == 1 && isFunction(%ray.getClassName(), "damage"))
		{
			%piercecheck = (!%pierce || getSimTime() - %obj.lastSwingStop < getSimTime() - %ray.tagged[%obj]);
			if(%piercecheck)
			{
				%targImg = %ray.getMountedImage(%slot);
				if (%this.MeleeCheckClash(%obj, %slot, %ray) || isObject(%targImg) && %targImg.MeleeCheckClash(%ray, %slot, %obj))
				{
					%start = %obj.getPosition();
					%end = %ray.getPosition();
					%velmult = %this.meleeBlockedVelocity;
					%ray.setVelocity(vectorAdd(vectorScale(vectorNormalize(vectorSub(%end, %start)), %velmult), "0 0 3"));
					%obj.setVelocity(vectorAdd(vectorScale(vectorNormalize(vectorSub(%start, %end)), %velmult), "0 0 3"));
					%ray.setImageLoaded(%slot, 0);
					%obj.setImageLoaded(%slot, 0);
					%ray.stopping = true; //Let 'em handle it proper
					%obj.stopping = true;
					%ray.stopThread(2);
					%obj.stopThread(2);
					%ray.swingPhase++;
					%obj.swingPhase++;
					%ray.schedule(%targImg.meleeBlockedStunTime * 1000, setImageLoaded, %slot, 1);
					%obj.schedule(%this.meleeBlockedStunTime * 1000, setImageLoaded, %slot, 1);
					%datablock = %this.meleeBlockedProjectile;
				}
				else
				{
					if(%this.meleeIsFreeform)
						%damage = %this.directDamage * (%swingspeed / 3); //num being the point where it multiplies the damage further
					%this.MeleeDamage(%obj, slot, %ray, %damage, %position);
					%datablock = %this.meleeHitPlayerProjectile;
					%ray.tagged[%obj] = getSimTime();
				}
			}
			else
				%datablock = "";
			%hitplayer = true;
		}

		if (isObject(%datablock) && (!%this.meleeSingleHitProjectile || !%ray.tagged[%obj] || %hitplayer || !%hitterrain))
		{
			%projectile = new Projectile()
			{
					datablock = %datablock;
					initialPosition = %position;
					initialVelocity = "0 0 0";
			};

			MissionCleanup.add(%projectile);

			%projectile.explode();

			for(%i = 0; %i < 4; %i++)
			{
				if(%this.meleeBounceAnim[%i] !$= "")
					%obj.playThread(%i, %this.meleeBounceAnim[%i]);
			}
		}

		if ((!%pierce && %hitplayer) || (!%hitplayer && !%this.meleePierceTerrain))//!%this.meleePierceTerrain && (!%hitplayer && !%pierce))
		{
			//talk("Badoop" SPC %this.meleePierceTerrain SPC %hitplayer SPC %pierce);
			%this.stopMeleeHitregLoop(%obj, %slot);
			return;
		}
		%hitterrain = true;
	}
	%last = %b;
	if (%frames != -1) %frames--;
	%obj.MeleeHitregLoop = %this.schedule(%this.meleeTick, "MeleeHitregLoop", %obj, %slot, %frames, %damage, %pierce, %last, %hitterrain);
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