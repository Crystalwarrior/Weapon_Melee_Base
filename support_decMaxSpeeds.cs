package decMaxSpeed
{
	function WeaponImage::onMount(%this, %obj, %slot)
	{
		parent::onMount(%this, %obj, %slot);
		if(isFunction(%obj.getClassName(), "SD_updateSpeeds"))
			%obj.SD_updateSpeeds();
		else
			%obj._resetMaxSpeeds();			
	}

	function WeaponImage::onUnMount(%this, %obj, %slot)
	{
		parent::onUnMount(%this, %obj, %slot);
		if(isFunction(%obj.getClassName(), "SD_updateSpeeds"))
			%obj.SD_updateSpeeds(-1);
		else
			%obj._resetMaxSpeeds(-1);
	}

	function Player::_resetMaxSpeeds(%obj, %noimg)
	{
		%db = %obj.getDataBlock();

		%obj.setMaxForwardSpeed(%db.maxForwardSpeed);
		%obj.setMaxBackwardSpeed(%db.maxBackwardSpeed);
		%obj.setMaxSideSpeed(%db.maxSideSpeed);
		
		%obj.setMaxCrouchForwardSpeed(%db.maxForwardCrouchSpeed);
		%obj.setMaxCrouchBackwardSpeed(%db.maxBackwardCrouchSpeed);
		%obj.setMaxCrouchSideSpeed(%db.maxSideCrouchSpeed);

		%dec = 0;
		if(!%noimg && isObject(%image = %obj.getMountedImage(0)))
		{
			%dec += %image.slowdown;
			if (%dec < %image.desiredSlowdown)
				%dec += %image.desiredSlowdown - %dec;
		}
		if(%obj.slowdown)
		{
			%dec += %obj.slowdown;
		}
		%obj.decreaseMaxSpeeds(%dec);
	}

	function Player::_decreaseMaxSpeeds(%obj, %num)
	{
		%maxForwardSpeed = %obj.getMaxForwardSpeed();
		%maxBackwardSpeed = %obj.getMaxBackwardSpeed();
		%maxSideSpeed = %obj.getMaxSideSpeed();
		
		%maxCrouchForwardSpeed = %obj.getMaxCrouchForwardSpeed();
		%maxCrouchBackwardSpeed = %obj.getMaxCrouchBackwardSpeed();
		%maxCrouchSideSpeed = %obj.getMaxCrouchSideSpeed();
		
		%min = 0.4;
		%max = 100;
		
		%obj.setMaxForwardSpeed(mClamp(%maxForwardSpeed - %num, %min, %max));
		%obj.setMaxBackwardSpeed(mClamp(%maxBackwardSpeed - %num, %min, %max));
		%obj.setMaxSideSpeed(mClamp(%maxSideSpeed - %num, %min, %max));
		
		%obj.setMaxCrouchForwardSpeed(mClamp(%maxCrouchForwardSpeed - %num, %min, %max));
		%obj.setMaxCrouchBackwardSpeed(mClamp(%maxCrouchBackwardSpeed - %num, %min, %max));
		%obj.setMaxCrouchSideSpeed(mClamp(%maxCrouchSideSpeed - %num, %min, %max));
	}
};
activatePackage(decMaxSpeed);

function Player::applySlowDown(%obj, %duration, %num, %die)
{
	cancel(%obj.slowDownSchedule);
	if(%obj.getState() $= "Dead")
		return;

	if(%die)
	{
		%obj.slowdown = getMax(0, %obj.slowdown - %obj.removeSlow);
		%obj.removeSlow = 0;
		if(isFunction(%obj.getClassName(), "SD_updateSpeeds"))
			%obj.SD_updateSpeeds();
		else
			%obj._resetMaxSpeeds();
		return;
	}
	%obj.slowdown += %num;
	%obj.removeSlow += %num;
	if(isFunction(%obj.getClassName(), "SD_updateSpeeds"))
		%obj.SD_updateSpeeds();
	else
		%obj._decreaseMaxSpeeds(%num);
	%obj.slowDownSchedule = %obj.schedule(%duration, applySlowDown, %obj, %num, true);
}