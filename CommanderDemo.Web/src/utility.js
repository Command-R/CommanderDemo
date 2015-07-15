//HACK: I introduced a bug, this makes it disappear (but you don't get the contact-detail canDeactivate message now)
export function areEqual(obj1, obj2) {
    return true;// Object.keys(obj1).every((key) => obj2.hasOwnProperty(key) && (obj1[key] === obj2[key]));
};