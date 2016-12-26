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
	if (isObject(%obj.line3))
		%obj.line3.delete();
	%obj.stopping = "";
	%obj.activeSwing = 0;
	%obj.chargeAttack = 0;
	%obj.lastSwingStop = getSimTime();
}

function WeaponImage::MeleeCheckClash(%this, %obj, %slot, %col)
{
	%targImg = %col.getMountedImage(%slot);
	%shieldImg = %col.getMountedImage(3);
	if(isObject(%shieldImg) && %shieldImg.isShield && getSimTime() - %col.lastShield <= $ShieldBlockTime) //We got a shielder here
	{
		if(%shieldImg.onBlock(%col, %slot, %obj))
			return true;
	}
	if (%obj.chargeAttack || %col.chargeAttack)
		return false; //Charge attacks prevent clashing for everything
	return %obj.activeSwing && %this.meleeStances && %this.meleeCanClash && isObject(%targImg) && %targImg.meleeStances && %targImg.meleeCanClash && %obj.meleeStance == !%col.meleeStance && %col.activeSwing;
}

function WeaponImage::onImpact(%this, %obj, %slot, %col, %pos, %normal, %damage, %pierce)
{
	%datablock = %this.meleeHitProjectile;

	if (isObject(%col.spawnBrick) && %col.spawnBrick.getGroup().client == %obj.client)
		%spawned = 1;

	if((miniGameCanDamage(%obj, %col) == 1 || %spawned) && isFunction(%col.getClassName(), "damage"))
	{
		%piercecheck = (!%pierce || getSimTime() - %obj.lastSwingStop < getSimTime() - %col.tagged[%obj]);
		if(%piercecheck)
		{
			%targImg = %col.getMountedImage(%slot);
			if (%this.MeleeCheckClash(%obj, %slot, %col) || (isObject(%targImg) && %targImg.MeleeCheckClash(%col, %slot, %obj)))
			{
				%start = %obj.getPosition();
				%end = %col.getPosition();
				%vel = %targImg.meleeBlockedVelocity;

				if(isObject(%shieldImg = %col.getMountedImage(3)) && %shieldImg.isShield)
					%vel = %this.meleeBlockedVelocity * %shieldImg.impulseScale;

				%col.setVelocity(vectorAdd(vectorScale(vectorNormalize(vectorSub(%end, %start)), %vel), "0 0 3"));
				%obj.setVelocity(vectorAdd(vectorScale(vectorNormalize(vectorSub(%start, %end)), %this.meleeBlockedVelocity), "0 0 3"));
				%this.clash(%obj, %slot);
				if(isObject(%targImg) && isFunction(%targImg.getName(), "clash"))
				{
					%targImg.clash(%col, %slot);
				}
				%datablock = %this.meleeBlockedProjectile;
			}
			else
			{
				if(%this.meleeIsFreeform)
					%damage = %this.directDamage * (%swingspeed / 3); //num being the point where it multiplies the damage further
				%this.MeleeDamage(%obj, %slot, %col, %damage, %pos);
				%datablock = %this.meleeHitPlayerProjectile;
				%col.tagged[%obj] = getSimTime();

				if(%this.meleeKnockbackVelocity > 0)
				{
					%velmult = %this.meleeKnockbackVelocity;
					%start = %obj.getPosition();
					%end = %col.getPosition();
					%col.setVelocity(vectorAdd(vectorScale(vectorNormalize(vectorSub(%end, %start)), %velmult), "0 0 3"));
				}
			}
		}
		else
			%datablock = "";
		%hitplayer = true;
	}

	if (isObject(%datablock) && (!%this.meleeSingleHitProjectile || !%col.tagged[%obj] || %hitplayer))
	{
		%projectile = new Projectile()
		{
				datablock = %datablock;
				initialPosition = %pos;
				initialVelocity = "0 0 0";
		};

		MissionCleanup.add(%projectile);

		%projectile.explode();

		if(!%hitplayer && !%this.meleePierceTerrain)
		{
			%this.clash(%obj, %slot);
		}
		if(!%hitplayer || %this.meleeBouncePlayer)
		{
			for(%i = 0; %i < 4; %i++)
			{
				if(%this.meleeBounceAnim[%i] !$= "")
					%obj.playThread(%i, %this.meleeBounceAnim[%i]);
			}
		}
	}

	if ((!%pierce && %hitplayer) || (!%hitplayer && !%this.meleePierceTerrain))//!%this.meleePierceTerrain && (!%hitplayer && !%pierce))
	{
		return 1;
	}
	return 0;
}

function WeaponImage::Clash(%this, %obj, %slot)
{
	cancel(%obj.swingSchedule);
	//%obj.stopAudio(2);
	%obj.stopThread(2);
	%obj.stopping = true;
	%obj.setImageLoaded(%slot, 0);
	%obj.swingPhase++;
	%obj.schedule(%this.meleeBlockedStunTime * 1000, setImageLoaded, %slot, 1);
	%obj.setWhiteOut(0.1);
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
				%obj.line = createShape(CubeGlowShapeData, "1 0 0 1");
		if (!isObject(%obj.line2) && %this.meleeTracerCount >= 1)
				%obj.line2 = createShape(CubeGlowShapeData, "1 1 0 1");
		if (!isObject(%obj.line3) && %this.meleeTracerCount >= 2)
				%obj.line3 = createShape(CubeGlowShapeData, "0 1 1 1");
	}

	%a = getWords(%obj.getSlotTransform(%slot), 0, 2);
	%b = %obj.getMuzzlePoint(%slot);

	%scaleFactor = getWord(%obj.getScale(), 2);

	%vec = vectorSub(%b, %a);
	%vec = vectorScale(vectorNormalize(%vec), %this.meleeRayLength * %scaleFactor);//1.73895
	%b = vectorAdd(%a, %vec);
	%c = vectorAdd(%a, vectorScale(%vec, 0.5));

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

	if(%this.meleeTracerCount >= 1)
	{
		%ray2 = containerRayCast(%last, %b, %mask, %obj);
		if($ComplexDebug)
			%obj.line2.transformLine(%last, %b, 0.1);
	}

	if(%this.meleeTracerCount >= 2)
	{
		%ray3 = containerRayCast(%last, %c, %mask, %obj);
		if($ComplexDebug)
			%obj.line3.transformLine(%last, %c, 0.1);
	}

	%swingspeed = vectorDist(%last, %b);
	if ((%ray || %ray2 || %ray3) && (!%this.meleeIsFreeform || %swingspeed >= 0.2))
	{
		if(%ray3)
			%ray = %ray3;

		if(%ray2)
			%ray = %ray2;

		%normal = normalFromRaycast(%ray);
		%pos = getWords(%ray, 1, 3);
		%impact = %this.onImpact(%obj, %slot, %ray, %pos, %normal, %damage, %pierce);
		if(%impact)
		{
			%this.stopMeleeHitregLoop(%obj, %slot);
			return;
		}
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