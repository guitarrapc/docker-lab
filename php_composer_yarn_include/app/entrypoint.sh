#!/bin/sh

# swap host vendor with image vendor (image vendor is right vendor dir)
if [[ -d "vendor" ]]
then
  rm -rf vendor/*
fi
cp -rf /tmp/vendor .

# swap host node_modules with image node_modules (image node_modules is right vendor dir)
if [[ -d "node_modules" ]]
then
  rm -rf node_modules/*
fi
cp -rf /tmp/node_modules .

# run
php-fpm