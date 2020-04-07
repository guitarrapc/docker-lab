#!/bin/bash

# swap host vendor with image vendor (image vendor is right vendor dir)
if [[ -d "vendor" ]]
then
  mv vendor/ /tmp/_temp
fi
cp -rf /tmp/vendor .

# run
php-fpm