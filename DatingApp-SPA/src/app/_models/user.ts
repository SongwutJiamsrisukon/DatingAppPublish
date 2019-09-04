import { Photo } from './photo';

export interface User {
    id: number; // use lower case in angular, In C# use title case for convention
    userName: string;
    knownAs: string;
    age: number;
    gender: string;
    created: Date;
    lastActive: Date;
    city: string;
    country: string;
    photoUrl: string;
    interests?: string; // optional property(alvis operator) must declare after required property
                        // if not you will get error
    introduction?: string;
    lookingFor?: string;
    photos?: Photo[];
    roles?: string[];
}
