/*
 * Copyright © 2007-2019 Matt Robinson
 *
 * SPDX-License-Identifier: GPL-3.0-or-later
 */

BEGIN TRANSACTION;

CREATE TABLE images
(
    imgid integer NOT NULL PRIMARY KEY AUTOINCREMENT,
    image blob
);

CREATE TABLE subscriptions
(
    progid integer UNIQUE NOT NULL REFERENCES programmes(progid)
);

CREATE TABLE favourites
(
    progid integer UNIQUE NOT NULL REFERENCES programmes(progid)
);

CREATE TABLE settings
(
    property varchar NOT NULL COLLATE nocase PRIMARY KEY,
    value varchar NOT NULL
);

CREATE TABLE tempfiles
(
    filepath varchar NOT NULL PRIMARY KEY
);

CREATE TABLE downloads
(
    epid integer UNIQUE NOT NULL REFERENCES episodes(epid),
    status integer NOT NULL DEFAULT 0,
    filepath varchar,
    errorcount integer NOT NULL DEFAULT 0,
    totalerrors integer NOT NULL DEFAULT 0,
    errortime datetime,
    errordetails varchar,
    playcount integer NOT NULL DEFAULT 0,
    errortype integer
);

CREATE TABLE programmes
(
    progid integer NOT NULL PRIMARY KEY AUTOINCREMENT,
    pluginid varchar NOT NULL,
    extid varchar NOT NULL,
    name varchar NOT NULL,
    lastupdate datetime,
    image integer REFERENCES images(imgid),
    description varchar,
    singleepisode NOT NULL DEFAULT 0,
    latestdownload datetime,
    UNIQUE (pluginid, extid)
);

CREATE TABLE episodes
(
    epid integer NOT NULL PRIMARY KEY AUTOINCREMENT,
    progid integer NOT NULL REFERENCES programmes(progid) ON DELETE CASCADE,
    extid varchar NOT NULL,
    name varchar NOT NULL,
    description varchar,
    duration integer,
    date datetime NOT NULL,
    image integer REFERENCES images(imgid),
    autodownload integer NOT NULL DEFAULT 1,
    available integer NOT NULL DEFAULT 0,
    UNIQUE (progid, extid)
);

CREATE TABLE episodeext
(
    epid integer NOT NULL REFERENCES episodes(epid) ON DELETE CASCADE,
    name varchar NOT NULL,
    value varchar NOT NULL,
    PRIMARY KEY (epid, name)
);

CREATE TABLE chapters
(
    epid integer NOT NULL REFERENCES episodes(epid) ON DELETE CASCADE,
    start integer NOT NULL,
    name varchar NOT NULL,
    link varchar,
    image integer REFERENCES images(imgid),
    PRIMARY KEY (epid, start)
);

COMMIT;
