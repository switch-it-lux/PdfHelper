﻿using System;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace Sitl.Pdf {

    //see: https://stackoverflow.com/a/57905252

    internal class TextChunkLocationDefaultImp : ITextChunkLocation {
        private const float DIACRITICAL_MARKS_ALLOWED_VERTICAL_DEVIATION = 2;

        /// <summary>the starting location of the chunk</summary>
        private readonly Vector startLocation;

        /// <summary>the ending location of the chunk</summary>
        private readonly Vector endLocation;

        /// <summary>unit vector in the orientation of the chunk</summary>
        private readonly Vector orientationVector;

        /// <summary>the orientation as a scalar for quick sorting</summary>
        private readonly int orientationMagnitude;

        /// <summary>perpendicular distance to the orientation unit vector (i.e.</summary>
        /// <remarks>
        /// perpendicular distance to the orientation unit vector (i.e. the Y position in an unrotated coordinate system)
        /// we round to the nearest integer to handle the fuzziness of comparing floats
        /// </remarks>
        private readonly int distPerpendicular;

        /// <summary>distance of the start of the chunk parallel to the orientation unit vector (i.e.</summary>
        /// <remarks>distance of the start of the chunk parallel to the orientation unit vector (i.e. the X position in an unrotated coordinate system)
        ///     </remarks>
        private readonly float distParallelStart;

        /// <summary>distance of the end of the chunk parallel to the orientation unit vector (i.e.</summary>
        /// <remarks>distance of the end of the chunk parallel to the orientation unit vector (i.e. the X position in an unrotated coordinate system)
        ///     </remarks>
        private readonly float distParallelEnd;

        /// <summary>the width of a single space character in the font of the chunk</summary>
        private readonly float charSpaceWidth;

        public TextChunkLocationDefaultImp(Vector startLocation, Vector endLocation, float charSpaceWidth) {
            this.startLocation = startLocation;
            this.endLocation = endLocation;
            this.charSpaceWidth = charSpaceWidth;
            Vector oVector = endLocation.Subtract(startLocation);
            if (oVector.Length() == 0) {
                oVector = new Vector(1, 0, 0);
            }
            orientationVector = oVector.Normalize();
            orientationMagnitude = (int)(Math.Atan2(orientationVector.Get(Vector.I2), orientationVector.Get(Vector.I1)
                ) * 1000);
            // see http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html
            // the two vectors we are crossing are in the same plane, so the result will be purely
            // in the z-axis (out of plane) direction, so we just take the I3 component of the result
            Vector origin = new Vector(0, 0, 1);
            distPerpendicular = (int)(startLocation.Subtract(origin)).Cross(orientationVector).Get(Vector.I3);
            distParallelStart = orientationVector.Dot(startLocation);
            distParallelEnd = orientationVector.Dot(endLocation);
        }

        public virtual int OrientationMagnitude() {
            return orientationMagnitude;
        }

        public virtual int DistPerpendicular() {
            return distPerpendicular;
        }

        public virtual float DistParallelStart() {
            return distParallelStart;
        }

        public virtual float DistParallelEnd() {
            return distParallelEnd;
        }

        /// <returns>the start location of the text</returns>
        public virtual Vector GetStartLocation() {
            return startLocation;
        }

        /// <returns>the end location of the text</returns>
        public virtual Vector GetEndLocation() {
            return endLocation;
        }

        /// <returns>the width of a single space character as rendered by this chunk</returns>
        public virtual float GetCharSpaceWidth() {
            return charSpaceWidth;
        }

        /// <param name="as">the location to compare to</param>
        /// <returns>true is this location is on the the same line as the other</returns>
        public virtual bool SameLine(ITextChunkLocation @as) {
            if (OrientationMagnitude() != @as.OrientationMagnitude()) {
                return false;
            }
            float distPerpendicularDiff = DistPerpendicular() - @as.DistPerpendicular();
            if (distPerpendicularDiff == 0) {
                return true;
            }
            LineSegment mySegment = new LineSegment(startLocation, endLocation);
            LineSegment otherSegment = new LineSegment(@as.GetStartLocation(), @as.GetEndLocation());
            return Math.Abs(distPerpendicularDiff) <= DIACRITICAL_MARKS_ALLOWED_VERTICAL_DEVIATION && (mySegment.GetLength
                () == 0 || otherSegment.GetLength() == 0);
        }

        /// <summary>
        /// Computes the distance between the end of 'other' and the beginning of this chunk
        /// in the direction of this chunk's orientation vector.
        /// </summary>
        /// <remarks>
        /// Computes the distance between the end of 'other' and the beginning of this chunk
        /// in the direction of this chunk's orientation vector.  Note that it's a bad idea
        /// to call this for chunks that aren't on the same line and orientation, but we don't
        /// explicitly check for that condition for performance reasons.
        /// </remarks>
        /// <param name="other"/>
        /// <returns>the number of spaces between the end of 'other' and the beginning of this chunk</returns>
        public virtual float DistanceFromEndOf(ITextChunkLocation other) {
            return DistParallelStart() - other.DistParallelEnd();
        }

        public virtual bool IsAtWordBoundary(ITextChunkLocation previous) {
            // In case a text chunk is of zero length, this probably means this is a mark character,
            // and we do not actually want to insert a space in such case
            if (startLocation.Equals(endLocation) || previous.GetEndLocation().Equals(previous.GetStartLocation())) {
                return false;
            }
            float dist = DistanceFromEndOf(previous);
            if (dist < 0) {
                dist = previous.DistanceFromEndOf(this);
                //The situation when the chunks intersect. We don't need to add space in this case
                if (dist < 0) {
                    return false;
                }
            }
            return dist > GetCharSpaceWidth() / 2.0f;
        }

        internal static bool ContainsMark(ITextChunkLocation baseLocation, ITextChunkLocation markLocation) {
            return baseLocation.GetStartLocation().Get(Vector.I1) <= markLocation.GetStartLocation().Get(Vector.I1) &&
                 baseLocation.GetEndLocation().Get(Vector.I1) >= markLocation.GetEndLocation().Get(Vector.I1) && Math.
                Abs(baseLocation.DistPerpendicular() - markLocation.DistPerpendicular()) <= DIACRITICAL_MARKS_ALLOWED_VERTICAL_DEVIATION;
        }
    }
}
