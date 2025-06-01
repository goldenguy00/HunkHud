## Changelog

`0.0.13`
- Ebony & Ivory bullet radius increased. Sometimes I Play This Game With A Controller And I Need This
- Prop damage: 2x300% > 400% + 200% - frontloading the damage just a tad, purely for band procs
- Shredder damage: 200% per tick > 250% per tick
- Bosses can no longer be juggled
- Fixed Lysate Cell doing nothing

`0.0.12`
- Drive now counts as a primary skill for that juicy Luminous Shot synergy

`0.0.11`
- Registered projectile to the catalog..... rookie mistake

`0.0.10`
- Added some more item displays
- Added footsteps
- Rebellion 3rd hit damage: 300% > 400%
- Helm Breaker damage: 500% > 600%
- Added Prop: Rebellion m1 > m1 > delay > m1, hits twice for 300% damage, stuns and launches enemies up
- Added Shredder: hold m1 during Prop to continue spinning your blade for 200%, tick rate scaling with attack speed
- Added Drive: Rebellion m1 > hold at any point while grounded to charge up and fire a sword beam for up to 1500% damage
- ^releasing Drive with proper timing boosts damage by 25%
- Fixed a rare issue causing multiple instances of the Devil Trigger ambient SFX to play forever

`0.0.9`
- 0.0.5 claimed to have added custom notifications for certain item effects - this was not actually working. Now it is.
- Added Enemy Step
- Hellbent now automatically activates Devil Trigger when procced- it was too easy to just straight up miss this, and not get any value out of it
- Rebellion is now an item
- Added aerial variant of Stinger- costs no gauge, has more endlag and doesn't travel as far
- Stinger no longer costs gauge while in Devil Trigger
- Deflecting projectiles no longer destroys them entirely, visuals improved
- VCR's Nero skins now use custom VFX

`0.0.8`
- Devil Trigger gauge generation on hit: 4% > 5%
- Stinger gauge cost: 10% > 5%


while the traversal issue was fixed, spending so much gauge in combat just using stinger felt like crap. with this change stinger refunds itself if you hit anything, and gauge generation is more consistent in general. more to come but this needed a quick hotfix

`0.0.7`
- Stinger movement speed greatly reduced
- Stinger new mechanic: now spends 10% gauge, if available, to use the previous speed
- Crosshair now rotates when charged shots are ready


stinger traversal was horribly toxic. this seemed the best way to handle it without nerfing fun anywhere, you no longer have to spam stinger across stages

`0.0.6`
- Updated survivor color and added it to some lang tokens for some added flavor
- Greatly reduced recoil on Ebony & Ivory - numbers were copy pasted from HUNK guns lol oops
- Ebony & Ivory charge shots no longer have bullet falloff
- Ebony & Ivory now uses different VFX during Devil Trigger
- Rebellion now deflects projectiles and bullets in front while held and not attacking
- Activating Devil Trigger now grants 0.5s of invulnerability
- Devil Trigger now heals more the lower your health is, up to 3x the base value
- Devil Trigger movespeed bonus: 2m/s > 3m/s
- Devil Trigger damage bonus: 25% > 30%
- Devil Trigger armor bonus: 50 > 100
- Added Devil Trigger passive: Hellbent - once per stage, upon falling to low health, instantly fill your gauge and gain a free Devil Trigger for the next 5 seconds


melee was feeling too risky a couple stages into a run, in part due to the only weapon currently being base Rebellion, so some of these number buffs may be pulled back in the future as devil arms are added

`0.0.5`
- Added notifications for all custom item effects
- Lysate Cells now boost maximum gauge by 10%. visuals aren't great for now so it may not feel the best
- Bandolier now fills up gauge by 25%
- Luminous Shot now charges as your weapon is held, losing all charge if you let go
- Fixed Backup Magazine's bonus resetting on respawn or next stage, fixing itself after picking up a new item

`0.0.4`
- WHY isn't this working? Must more blood be shed?

`0.0.3`
- Lock-On camera is now disabled by default. existing installs are unaffected. just feels more natural this way
- Lock & Load with lock-on disabled now slightly lowers camera sensitivity, amount is configurable
- Fixed Stinger and Trick still homing into enemies, and the Lock-On indicator being visible with Lock-On disabled

`0.0.2`
- Added config option to make Lock & Load a toggle, rather than needing to be held
- Disabling Lock & Load's camera movement in the config now allows free camera movement while held
- Melee combos now require key presses to continue the combo- moves with hold inputs will come so this needed to be done asap
- Trick now dashes directly to the target if you input forward while locked on
- Rebellion 3rd hit now has knockback and a slightly longer recovery
- Stinger no longer knocks enemies away- this is how it works in DMC,
- Purity now increases the amount of gauge earned on hit by a flat 6% per stack (2% if Green Alien Head is installed)
- Alien Head now lowers the Devil Trigger gauge tick rate, using the same hyperbolic scaling (and reduced if Green Alien Head is installed)
- Brainstalks now prevents Devil Trigger from consuming any gauge while active
- Backup Magazines now grant the ability to store extra charge shots, consuming them one at a time
- Fixed Ebony & Ivory description incorrectly stating it takes 2 seconds to charge- it's actually only 1

`0.0.1`
- Beta release