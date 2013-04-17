@echo off

iff "%*" eq "" then

  echo ``
  echo ``
  echo must call git-update with a commit message, all arguments are used
  echo ``
  goto DONE

else

  call git-add
  call git commit -m "%*"
  
::
:: call this explicitly
::
::  call git-push

endiff





:DONE


