package decMaxSpeed
{
	function Player::resetMaxSpeeds(%obj)
	{
		%db = %obj.getDataBlock();

		%obj.setMaxForwardSpeed(%db.maxForwardSpeed);
		%obj.setMaxBackwardSpeed(%db.maxBackwardSpeed);
		%obj.setMaxSideSpeed(%db.maxSideSpeed);
		
		%obj.setMaxCrouchForwardSpeed(%db.maxForwardCrouchSpeed);
		%obj.setMaxCrouchBackwardSpeed(%db.maxBackwardCrouchSpeed);
		%obj.setMaxCrouchSideSpeed(%db.maxSideCrouchSpeed);
	}

	function Player::decreaseMaxSpeeds(%obj, %num)
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

if(!isPackage(SD_MedArmorPackage))
	activatePackage(decMaxSpeed);

function Player::applySlowDown(%obj, %duration, %num, %die)
{
	cancel(%obj.slowDownSchedule);
	if(%obj.getState() $= "Dead")
		return;

	if(%die)
	{
		%obj.slowdown -= %num;
		if(isFunction(%obj, "SD_updateSpeeds"))
			%obj.SD_updateSpeeds();
		else
			%obj.resetMaxSpeeds();
		return;
	}
	%obj.slowdown += %num;
	if(isFunction(%obj, "SD_updateSpeeds"))
		%obj.SD_updateSpeeds();
	else
		%obj.decreaseMaxSpeeds(%num);
	%obj.slowDownSchedule = %obj.schedule(%duration, applySlowDown, %obj, %num, true);
}