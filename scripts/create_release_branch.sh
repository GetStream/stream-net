#!/bin/bash

VERSION=$1
echo "Preparing release $VERSION"

# Update .csproj file
# This regex to update Version tag in .csproj file
sed -i '' 's|<Version>\(.*\)</Version>|<Version>'"${VERSION}"'</Version>|g' src/stream-net.csproj


# Create changelog
# --skip.commit: We manually commit the changes
# --skip-tag: tagging will done by the GitHub release step, so skip it here
# --tag-prefix: by default it tries to compare v0.1.0 to v0.2.0. Since we do not prefix our tags with 'v'
# we set it to an empty string
npx --yes standard-version@9.3.2 --release-as "$VERSION" --skip.tag --skip.commit --tag-prefix=

git config --global user.name 'github-actions'
git config --global user.email 'release@getstream.io'
git checkout -q -b "release-$VERSION"
git commit -am "chore(release): $VERSION"
git push -q -u origin "release-$VERSION"

echo "Done!"
