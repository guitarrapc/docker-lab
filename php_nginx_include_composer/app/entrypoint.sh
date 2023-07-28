#!/bin/sh

# swap host vendor with image vendor (image vendor is right vendor dir)
if [[ -d "vendor" ]]
then
  rm -rf vendor/*
fi
cp -rf /tmp/vendor .

# run
php-fpm