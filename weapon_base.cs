package MeleeBasePackage
{
	//function WeaponImage::onFire(%this, %obj, %slot)
	//{
	//	%obj.swingPhase = (%obj.swingPhase + 1) % 2;
	//	%obj.playthread(2, %this.meleeAnimation @ %obj.swingPhase + (%obj.meleeStance ? 3 : 1));
	//	%this.schedule(16, MeleeHitregLoop, %obj, %slot, 12);
	//}

	function WeaponImage::onUnMount(%this, %obj, %slot)
	{
		if(!%this.meleeEnabled)
			return parent::onUnMount(%this, %obj, %slot);
		%this.setChargeSlowdown(%obj, 0);
		parent::onUnMount(%this, %obj, %slot);
	}

	function WeaponImage::onMount(%this, %obj, %slot)
	{
		parent::onMount(%this, %obj, %slot);
		%props = %obj.getItemProps();
		if(%props.class !$= "MeleeWeaponProps")
			return;

		if(%props.durability $= "" && %props.sourceItemData.durability $= "")
			return;

		if(%props.durability <= 0)
		{
			%pos = %obj.getSlotTransform(%slot);
			%obj.tool[%props.itemSlot] = "";
			if (isObject(%obj.client))
				messageClient(%obj.client, 'MsgItemPickup', '', %props.itemSlot, 0);
			%props.delete();

			%projectile = new Projectile()
			{
				datablock = MeleeBreakProjectile;
				initialPosition = %pos;
				initialVelocity = "0 0 0";
			};
			MissionCleanup.add(%projectile);
			%projectile.explode();

			%obj.unMountImage(0);
			return;
		}
		else if(isObject(%client = %obj.client))
		{
			if(%props.durability <= (%this.item.durability / 2))
				%msg = "Your weapon feels flimsy.";
			if(%props.durability <= (%this.item.durability / 4))
				%msg = "Your weapon feels very flimsy!";
			if(%props.durability <= 10)
				%msg = "Your weapon is about to break!";
			if(%msg !$= "")
				messageClient(%client, '', '\c4WARNING\c3: %1', %msg);
		}
	}

	function Armor::onTrigger(%data, %this, %trig, %tog)
	{
		//if (%trig == 2)
		//{
		//	%this.schedule(0, meleeJumpLoop, %tog);
		//}

		Parent::onTrigger(%data, %this, %trig, %tog);
		%image = %this.getMountedImage(0);
		if (!isObject(%image) || !%image.meleeEnabled)
			return;
		if (%image.meleeStances && %trig == 4 && %tog)
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

//function Player::meleeJumpLoop(%this, %tog)
//{
//	cancel(%this.meleeJumpLoop);
//	if(!%tog)
//		return;
//	if(%this.getState() $= "Dead")
//		return;
//
//	%data = %this.getDataBlock();
//	%delay = 100;
//	if(isObject(%this.getMountedImage(0)) && %drain = %this.getMountedImage(0).meleeJumpDrain)
//	{	
//		%delay = %data.jumpDelay * 32; //A player tick is 32ms
//		if(%this.getEnergyLevel() >= %data.maxEnergy && getWord(%this.getVelocity(), 2) > 0)
//			%this.setEnergyLevel(%this.getEnergyLevel() - %drain);
//		else
//			%delay = 100;
//	}
//	talk(%delay SPC getWord(%this.getVelocity(), 2));
//
//	%this.meleeJumpLoop = %this.schedule(%delay, meleeJumpLoop, %tog);
//}


function WeaponImage::setChargeSlowdown(%this, %obj, %tog)
{
	if(%tog)
		%obj.desiredSlowdown = 3;
	else
		%obj.desiredSlowdown = 0;
	if(%tog && getWord(%obj.getVelocity(), 2) > 0)
		%obj.setVelocity(setWord(%obj.getVelocity(), 2, 0));
	%obj.SD_updateSpeeds();
	if((%data = %obj.getDataBlock()) == nameToID("SurvivorArmor"))
	{
		%data.setCanJump(%obj, !%tog);
	}
}

function WeaponImage::onStanceSwitch(%this, %obj, %slot)
{
}

function WeaponImage::stopMeleeHitregLoop(%this, %obj, %slot)
{
	cancel(%obj.swingSchedule);
	cancel(%obj.MeleeHitregLoop);
	if (isObject(%obj.line))
		%obj.line.delete();
	if (isObject(%obj.line2))
		%obj.line2.delete();
	if (isObject(%obj.line3))
		%obj.line3.delete();
	if (isObject(%obj.line4))
		%obj.line4.delete();
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
	if(%damage <= 0)
		%datablock = MeleeStickBlockProjectile;

	if (isObject(%col.spawnBrick) && %col.spawnBrick.getGroup().client == %obj.client)
		%spawned = 1;

	if((miniGameCanDamage(%obj, %col) == 1 || %spawned) && isFunction(%col.getClassName(), "damage"))
	{
		%piercecheck = (!%pierce || getSimTime() - %obj.lastSwingStop < getSimTime() - %col.tagged[%obj]);
		if(%piercecheck)
		{
			if(%damage > 0)
			{
				%targImg = %col.getMountedImage(%slot);
				if (%this.MeleeCheckClash(%obj, %slot, %col) || (isObject(%targImg) && %targImg.MeleeCheckClash(%col, %slot, %obj)))
				{
					%start = %obj.getPosition();
					%end = %col.getPosition();
					%vel = %targImg.meleeBlockedVelocity;

					%col.setVelocity(vectorAdd(vectorScale(vectorNormalize(vectorSub(%end, %start)), %vel), "0 0 3"));
					%obj.setVelocity(vectorAdd(vectorScale(vectorNormalize(vectorSub(%start, %end)), %this.meleeBlockedVelocity), "0 0 3"));
					if(isObject(%shieldImg = %col.getMountedImage(3)) && %shieldImg.isShield && getSimTime() - %col.lastShield <= $ShieldBlockTime)
					{
						%vel = %this.meleeBlockedVelocity * %shieldImg.impulseScale;
					}
					else
					{
						%this.clash(%obj, %slot);
						if(isObject(%targImg) && isFunction(%targImg.getName(), "clash"))
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
				%hitplayer = true;
			}
			else
			{
				%pierce = 0;
			}
		}
		else
		{
			%datablock = "";
			%hitplayer = true;
		}
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

	%props = %obj.getItemProps();
	if(%props.durability !$= "" || %props.sourceItemData.durability !$= "")
	{
		if(%props.class $= "MeleeWeaponProps")
			%props.durability--;

		if(%props.durability <= 0)
		{
			%pos = %obj.getSlotTransform(%slot);
			%obj.tool[%props.itemSlot] = "";
			if (isObject(%obj.client))
				messageClient(%obj.client, 'MsgItemPickup', '', %props.itemSlot, 0);
			%props.delete();

			%projectile = new Projectile()
			{
				datablock = MeleeBreakProjectile;
				initialPosition = %pos;
				initialVelocity = "0 0 0";
			};
			MissionCleanup.add(%projectile);
			%projectile.explode();

			%obj.unMountImage(0);
			return 1;
		}
	}

	if ((!%pierce && %hitplayer) || (!%hitplayer && !%this.meleePierceTerrain))//!%this.meleePierceTerrain && (!%hitplayer && !%pierce))
	{
		return 1;
	}
	return 0;
}

function WeaponImage::Clash(%this, %obj, %slot, %duration)
{
	%this.stopMeleeHitregLoop(%obj, %slot);
	if(%duration $= "")
		%duration = %this.meleeBlockedStunTime;
	//%obj.stopAudio(2);
	%obj.stopThread(2);
	%obj.stopping = true;
	%obj.setImageLoaded(%slot, 0);
	%obj.swingPhase++;
	cancel(%obj.clashSchedule);
	%obj.clashSchedule = %obj.schedule(%duration * 1000, setImageLoaded, %slot, 1);
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
	
	if(%damage $= "")
		%damage = %this.directDamage;

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
		if (!isObject(%obj.line4) && %this.meleeTipDamage)
				%obj.line4 = createShape(CubeGlowShapeData, "1 0 1 1");
	}

	%a = getWords(%obj.getSlotTransform(%slot), 0, 2);
	%b = %obj.getMuzzlePoint(%slot);

	%scaleFactor = getWord(%obj.getScale(), 2);

	%vec = vectorSub(%b, %a);
	%vec = vectorScale(vectorNormalize(%vec), %this.meleeRayLength * %scaleFactor);//1.73895
	%b = vectorAdd(%a, %vec);

	%tip = %this.meleeTipFactor;
	if(%this.meleeTipFactor $= "")
		%tip = 0.5;
	%c = vectorAdd(%a, vectorScale(%vec, %tip));

	%mask =
			$TypeMasks::FxBrickObjectType |
			$TypeMasks::TerrainObjectType |
			$TypeMasks::StaticShapeObjectType |
			$TypeMasks::VehicleObjectType |
			$TypeMasks::PlayerObjectType;

	if(%this.meleeTipDamage)
	{
		%ray = containerRayCast(%c, %b, %mask, %obj);
		%ray4 = containerRayCast(%a, %c, %mask, %obj);
		if($ComplexDebug)
		{
			%obj.line.transformLine(%c, %b, 0.1);
			%obj.line4.transformLine(%a, %c, 0.1);
		}
	}
	else
	{
		%ray = containerRayCast(%a, %b, %mask, %obj);
		if($ComplexDebug)
			%obj.line.transformLine(%a, %b, 0.1);
	}

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
	if ((%ray || %ray2 || %ray3 || %ray4) && (!%this.meleeIsFreeform || %swingspeed >= 0.2))
	{
		if(%ray3)
			%ray = %ray3;

		if(%ray2)
			%ray = %ray2;

		if(%ray4) //"stick" ray
		{
			if(%this.meleeTipDamage)
				%damage = 0;
			%ray = %ray4;
		}

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