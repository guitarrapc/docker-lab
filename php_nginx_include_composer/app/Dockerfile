FROM php:7.4-fpm-alpine

RUN apk update \
  && apk add git libzip-dev unzip \
  && docker-php-ext-install zip

COPY --from=composer:latest /usr/bin/composer /usr/bin/composer
ENV COMPOSER_ALLOW_SUPERUSER 1
ENV COMPOSER_HOME /composer
ENV PATH $PATH:/composer/vendor/bin

WORKDIR /tmp

COPY ./app/composer.json ./composer.json
COPY ./app/composer.lock ./composer.lock
RUN composer install

WORKDIR /var/www/html

COPY ./app/entrypoint.sh .
CMD ["./entrypoint.sh"]