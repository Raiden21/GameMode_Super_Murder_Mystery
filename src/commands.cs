function serverCmdYes(%this)
{
	if($Sim::Time - %this.lastVO > 1 && isObject(%this.player))
	{
		if(!%this.smmGender)
		{
			serverPlay3d(MaleYesSound, %this.player.getEyePoint());
		}
		else
		{
			serverPlay3d(FemaleYesSound, %this.player.getEyePoint());
		}
		%this.lastVO = $Sim::Time;
		%text = "Yeah.";
		serverCmdMessageSent(%this, %text);
	}
}

function serverCmdNo(%this)
{
	if($Sim::Time - %this.lastVO > 1 && isObject(%this.player))
	{
		if(!%this.smmGender)
		{
			serverPlay3d(MaleNoSound, %this.player.getEyePoint());
		}
		else
		{
			serverPlay3d(FemaleNoSound, %this.player.getEyePoint());
		}
		%this.lastVO = $Sim::Time;
		%text = "No!";
		serverCmdMessageSent(%this, %text);
	}
}

function serverCmdThanks(%this)
{
	if($Sim::Time - %this.lastVO > 1 && isObject(%this.player))
	{
		if(!%this.smmGender)
		{
			serverPlay3d(MaleThanksSound, %this.player.getEyePoint());
		}
		else
		{
			serverPlay3d(FemaleThanksSound, %this.player.getEyePoint());
		}

		%this.lastVO = $Sim::Time;
		%text = "Thanks!";
		serverCmdMessageSent(%this, %text);
	}
}

function serverCmdHelp(%this)
{
	if($Sim::Time - %this.lastVO > 1 && isObject(%this.player))
	{
		if(!%this.smmGender)
		{
			serverPlay3d(MaleHelpSound, %this.player.getEyePoint());
		}
		else
		{
			serverPlay3d(FemaleHelpSound, %this.player.getEyePoint());
		}
		%this.lastVO = $Sim::Time;
		%text = "!Help!!";
		serverCmdMessageSent(%this, %text);
	}
}

function serverCmdWtf(%this)
{
	if($Sim::Time - %this.lastVO > 2 && isObject(%this.player))
	{
		%rnd = getRandom(1, 2);
		if(!%this.smmGender)
		{
			serverPlay3d(MaleWtfSound @ %rnd, %this.player.getEyePoint());
		}
		else
		{
			serverPlay3d(FemaleWtfSound @ %rnd, %this.player.getEyePoint());
		}

		%this.lastVO = $Sim::Time;
		%text = (%rnd == 2 ? "What the hell was that crap?!" : "What the hell is you guys' problem?!");
		serverCmdMessageSent(%this, %text);
	}
}

function serverCmdSuccumb(%this)
{
	if(isObject(%this.corpse) && %this.corpse.isUnconscious)
	{
		%this.corpse.isUnconscious = false;
		messageClient(%this, '', "<color:AAAAAA>You have given up life and succumbed to death.");
	}
}

function serverCmdDie(%this)
{
	serverCmdSuccumb(%this); //Redirect
}